# Pocket ID Sync Tool

A command-line utility designed to synchronize OIDC Client and User Group definitions between local YAML files and a [Pocket ID](https://pocket-id.org/) instance.

## Goal & Use Case

The primary goal of this tool is to provide a **declarative approach** to managing OIDC resources. By using YAML definitions modeled after Kubernetes Custom Resource Definitions (CRDs), users can manage their OIDC infrastructure as code.

- **Bidirectional Sync**: Synchronize local YAML state to a Pocket ID instance, or export existing Pocket ID configurations to local files.

- **Kubernetes-Ready**: This CLI serves as a proof-of-concept (PoC) and testing ground for an upcoming Kubernetes Operator.

- **Infrastructure Automation**: Designed to automate the creation of OIDC clients during application deployment (e.g., via Helm or ArgoCD).

## Scope

- **In Scope**: OIDC Client definitions and User Group definitions.

- **Out of Scope**: User management and group memberships. We consider user identity management to be the domain of Pocket ID’s native LDAP integration; this tool focuses strictly on the application/service layer.

## Current State

**Status: Early Beta**

The core synchronization logic is functional. Current development is focused on stabilizing the integration with the Pocket ID API and refining the schema to ensure compatibility with future Kubernetes-native implementations.

> [!WARNING] As this is an early beta, the YAML schemas are subject to change.


# Quick start

## 1. Installation

Download the appropriate binary for your operating system from the Releases page. Ensure the binary is in your system PATH.

## 2. Authentication

Log in to your Pocket ID instance and generate an API Key via the settings dashboard.

## 3. Usage

The CLI is structured into two main resource commands: `oidc-client` and `user-groups`. Use the `--help` flag at any level to see available options.

```shell
# General help
PocketIdSync --help

# Resource-specific help
PocketIdSync oidc-client --help
```

## 4. Examples

To list existing OIDC clients from your instance:

```shell
PocketIdSync oidc-client list --pocket-id-uri https://idp.example.com --api-key your_secret_key_here
```

**Safety First: Dry Run**
Since this tool manages infrastructure state, it includes a `--dry-run` flag. We strongly recommend using this to preview changes before they are applied to your Pocket ID instance.

To sync **Pocket ID** to your local configuration (e.g. git repository):

```shell
PocketIdSync oidc-client sync --synchronize Configuration --store-root ./local-config --pocket-id-uri https://idp.example.com --api-key your_secret_key_here --dry-run
```

To sync your local configuration to the **Pocket ID** server:

```shell
PocketIdSync oidc-client sync --synchronize PocketID --store-root ./local-config --pocket-id-uri https://idp.example.com --api-key your_secret_key_here --dry-run
```

> [!NOTE] Synching even a modest number of clients will propably trigger Pocket ID's rate limiting. You may need to change  DISABLE_RATE_LIMITING to true.

# Resource Definitions

## UserGroup

```yaml
apiVersion: pocketid.closure.ch/v1
kind: UserGroup
metadata:
  name: family # Unique identifier for the local resource
  namespace: default # Logical grouping (prepared for K8s compatibility, currently not used)
spec:
  name: family # The internal name in Pocket ID (not the Id)
  friendlyName: Family # The display name shown in the UI
  customClaims: # Optional: OIDC claims associated with this group
    - key: locale
      value: de-CH
```

## OIDCClient

```yaml
apiVersion: pocketid.closure.ch/v1
kind: OidcClient
metadata:
  name: an-example-application
  namespace: default
spec:
  callbackURLs:
    - https://an-app.example.com/login
  credentials: {}
  id: an-example-application
  isPublic: true
  launchURL: https://an-app.example.com/
  logoutCallbackURLs:
    - https://an-app.example.com/logout
  name: An example Application
  pkceEnabled: true
  requiresReauthentication: false
  allowedGroups:
    - public
    - family
  logoPath: an-example-application.jpg
  logoContent: !!binary |-
    /9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAMCAgMCAgMDAwMEAwMEBQgFBQQEBQoHBwYIDAoMDAsK
    ...
  logoDarkPath: an-example-application-dark.svg
  logoDarkContent: !!binary |-
    PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI2NDAiIGhlaWdo
    ...
```

- **Optional Fields**: All values under spec are technically optional.

- **User group Dependencies**: If `allowedGroups` are specified, the groups must already exist in Pocket ID. The CLI will validate this and return an error if a group definition is missing. The allowed groups correspond to the name (not Id) of the user group.

- **Secret Management**: When an OIDC client is configured as private (`isPublic: false`), Pocket ID generates a Client Secret.
  - **Kubernetes Compatibility**: The CLI automatically saves this secret locally in a dedicated file named `<client-name>-Secret.yaml`, formatted as a standard Kubernetes Secret.
  - **One-Time Retrieval**: Due to security constraints, the Pocket ID API (and Web UI) does not allow retrieval of the secret after creation.
  - **Security Warning**: Ensure these generated secret files are stored securely (e.g., in a Secret Manager or encrypted via SOPS/Vault). If the local secret file is lost, you will need to regenerate the secret within Pocket ID and update your application.

- **Logo Management**:
  - Pocket ID supports separate logos for light and dark modes.
  - **Inline**: You can provide the image data directly in the YAML using logoContent (base64/binary).
  - **Sidecar** (CLI Only): For local management, you can reference external files. When syncing to local files, you can choose between inline content (default) or sidecar files via a CLI flag.

> [!NOTE] Sidecar file support is exclusive to the CLI and will not be supported in the future Kubernetes Operator.

## Local configuration store

Currently the local configuration must be stored in a directory (in the example named `root`) having following subdirectories
~~~yaml
root
    default
        oidcClient
        userGroup
~~~
> [!WARNING] The directory structure is subject to change.

## Disclaimer

This project is **not affiliated** with the official Pocket ID project. It is an independent tool that interacts with the Pocket ID instance via its public API.

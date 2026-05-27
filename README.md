# 🐳 TotalCommander Docker/Kubernetes/Podman Plugin

**TotalCommander Docker/Kubernetes/Podman Plugin** is a Total Commander extension that allows you to browse and interact with Docker, Podman, and Kubernetes containers as if they were part of the local filesystem.

---

## ✨ Features

- 📂 Browse Docker containers as a filesystem
- 📂 Browse Podman containers as a filesystem
- ⌨️ Execute commands via Total Commander’s command line
- 🗂️ File operations:
    - Create
    - Delete
    - Rename
    - Move
- 📁 Directory operations:
    - Create
    - Delete
    - Rename
    - Move
- 📑 Open files with the default application (copies to `%TEMP%` first)

> ⚠️ **Limitation:** The plugin cannot persist file changes back into the container.

---

## 🧩 Kubernetes Plugin Support

The Kubernetes plugin provides the **same functionality and workflow** as the Docker plugin:

- Browse clusters, namespaces, pods, and containers
- Navigate the container filesystem seamlessly
- Perform the same file and directory operations: create, delete, rename, move
- Execute commands in the selected pod/container via the Total Commander command line

If you are familiar with the Docker plugin, you can use the Kubernetes plugin in the **exact same way**, with no new learning curve.

---

## Podman Plugin Support

The Podman plugin provides the same container filesystem workflow as the Docker plugin using the Podman CLI:

- Browse running Podman containers as directories
- Navigate container filesystems
- Create, delete, rename, move, and copy files/directories
- Execute commands in the selected container via Total Commander's command line

---

## 📥 Installation

1. Unpack `docker/podman/kubernetes.zip`
2. In Total Commander, go to:
   `Configuration -> Options -> Plugins -> WFX Plugins -> Add`
3. Select the plugin file `docker.wfx`, `podman.wfx`, or `k8s.wfx` matching your target and architecture
4. Open the `//` directory inside Total Commander to access containers

---

## 🚀 Roadmap

- Editing files directly inside containers
- Performance improvements
- Extended Kubernetes cluster browsing features

---

## 📌 Latest Release

👉 [Download the latest version](https://github.com/maximiliysiss/tc_docker/releases/latest)

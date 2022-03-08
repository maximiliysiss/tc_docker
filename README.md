# TotalCommander Docker/Kubernetes Plugin

TotalCommander Docker/Kubernetes Plugin is plugin for total commander to interact with docker containers

## TotalCommander.DockerPlugin

TotalCommander.DockerPlugin is plugin for total commander to interact with docker containers

Features:

1. Show docker containers like file system
2. Redirect commands from TotalCommander
3. Delete files
4. Create directories
5. Open file by default program

> **Notes:**<br>
> Plugin cannot save changes<br>
> Plugin cannot create new file - using `touch {filename}` in TotalCommander command line<br>
> Plugin show file size in linux block-size

### Installation

> **Last build archives:** <br>
> [Plugin](https://github.com/maximiliysiss/ts_docker/master/artifacts/docker) <br>
> [Environment](https://github.com/maximiliysiss/ts_docker/master/artifacts/install) <br>

#### Instruction

1. Unzip `installaction.zip`
2. Run `TcPluginLib.msi`
3. Unzip `plugin.zip`
4. Copy `TotalCommander.DockerPlugin.Adapter.dll` to TC root folder
5. Open `TC -> Configuration -> Options -> Plugins -> WFX Plugins -> Add`
6. Add new plugin
7. Go to `//`

### Build

***Environment:***

| Technology | Environment |
|:----------:|:-----------:|
|    .NET    |    4.7.2    |

### Future features

1. Plugin for Kubernetes
2. Add editing for files
3. Print correct size of files. Now print linux block-size for file instead of byte size
4. Copy/Move from/to container
5. Rename

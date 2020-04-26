# Configuring Linux build agent for PskOnline

## Before you begin

* Check the names of the  build agent in the scrips, when deploying additional build agents.
* See 'Notes' section at the bottom for some more details.

## The Manual

#### Install Ubuntu 18.04
    DO NOT INSTALL DOCKER, k8s OR DOCKER-COMPOSE!
    Install SSH Server
    Configure drives as LVM (use entire disk)
    Increase size of main volume to 20 GB

#### Prerequisites

* git (must have)
* Midnight Commander (nice to have)

#### Update apt & upgrade system
```
    sudo apt update
    sudo apt upgrade
```
### Clone repository with setup files
```
    git clone https://github.com/adadurov/PskOnline.git
```

### Setup build agent components

 Change to PskOnline/teamcity and run: 

```
    sudo agent-ubuntu-18.sh
```

At this point, this script is an interactive one, so be prepared to confirm some of the actions.

This script installs the following components:

* Docker Community Edition
* Docker Compose

It also prepares a data directory in /var/teamcity/

### Manual installation of Docker CE and Docker Compose

#### Install docker-ce (community edition)

https://www.digitalocean.com/community/tutorials/how-to-install-and-use-docker-on-ubuntu-18-04

#### Install docker-compose
```
    sudo apt install docker-compose
```

### How to heck size of LVM volume and extend if needed
```
    sudo lvdisplay
    sudo lvextend -l +4096 /dev/ubuntu-vg/ubuntu-lv ... (check the output of lvdisplay above)
    sudo resize2fs /dev/ubuntu-vg/ubuntu-lv ... (check the output of lvdisplay above)
```

### Install mc (midnight commander)
```
    sudo add-apt-repository universe
    sudo apt-get update
    sudo apt install mc
```

#### Prepare directories for persisting Teamcity build agent data
```
    sudo mkdir /var/teamcity
    sudo mkdir /var/teamcity/psk-bs-01-dock01
    sudo chmod a+rwx -R /var/teamcity
```

#### Compose and run the containerized environment
```
    sudo docker-compose up -d
```

* Run `docker-compose ps` to validate that containers are running.

#### Validate the setup

* Authorize the build agent in Teamcity, if needed.
* Run build in TeamCity.
* Troubleshoot any failing tests.
* Reboot the system to validate that the containers start automatically on startup.

#### Notes
```
build-agent/Dockerfile
```
-- the spec for building an image named 'teamcity_node_agent' including TeamCity build agent, Node.JS and .Net Core 2.1.1 (AKA 2.1.301)

```
docker-compose.yml
```
-- the spec for running the docker-compose environment including the Teamcity agent and MS SQL for Linux (for test databases only, with no persistent data)

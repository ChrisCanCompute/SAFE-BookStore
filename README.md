# SAFE postgres migrations

This fork of the [SAFE bookstore project](https://github.com/SAFE-Stack/SAFE-BookStore) shows:
* How to ru nthe app on kuberentes
* How to setup Postgres storage
* How to run Postgres migrations
* How to run a regular cleaning job

## Setup - Running in kubernetes

1. Install [Docker desktop](https://www.docker.com/products/docker-desktop) - includes k8s and a local docker repo
1. In Docker desktop, enable kubernetes:  
Settings > enable > restart
1. Install any [SAFE bookstore requirments](https://github.com/SAFE-Stack/SAFE-BookStore#requirements) you don't already have.
1. Build the project  
```./build.cmd run```
1. Deploy the project  
```./build.cmd BundleClient```
1. Build the docker file  
```docker build -t book-store .```
1. Deploy to docker  
```docker run -d -p 80:8085 book-store```   
Open http://localhost:80 to see it running.
1. Run it on kubernetes   
```kubectl apply -f .\singleApp.yaml```   
```kubectl expose deployment book-store --type=LoadBalancer```   
http://localhost:8085/
Note that simpleApp.yaml specifies two nodes, which each have their own local backing, so you are servered different data depending on which node you are routed to.

# Next steps

1. Check out the branch `part1` to continue the tutorial.

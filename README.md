# SAFE postgres migrations

This fork of the [SAFE bookstore project](https://github.com/SAFE-Stack/SAFE-BookStore) shows:
* How to ru nthe app on kuberentes
* How to setup Postgres storage
* How to run Postgres migrations
* How to run a regular cleaning job

## Setup

1. Install [Docker desktop](https://www.docker.com/products/docker-desktop) - includes k8s and a local docker repo
1. In Docker desktop, enable kubernetes:  
Settings > enable > restart
1. Install any [SAFE bookstore requirments](https://github.com/SAFE-Stack/SAFE-BookStore#requirements) you don't already have.

## Part0 - Creating a docker container

1. Bundle the project  
```./build.cmd BundleClient```
1. Build the docker file  
```docker build -t book-store .```
1. Deploy to docker  
```docker run -d -p 80:8085 book-store```   
Open http://localhost:80 to see it running.

# Next steps

1. [Part1](https://github.com/ChrisCanCompute/SAFE-BookStore/blob/Part1/README.md#part1---creating-a-kubernetes-deployment) - Creating a kubernetes deployment
1. [Part2](https://github.com/ChrisCanCompute/SAFE-BookStore/blob/Part2/README.md#part2---add-postgres-storage-to-the-app) - Add Postgres storage to the app
1. [Part3](https://github.com/ChrisCanCompute/SAFE-BookStore/blob/Part3/README.md#part3---add-postgres-migrations) - Add Postgres migrations
1. [Part4](https://github.com/ChrisCanCompute/SAFE-BookStore/blob/Part4/README.md#part4---create-a-cleaning-app) - Create a cleaning app
1. [Part5](https://github.com/ChrisCanCompute/SAFE-BookStore/blob/Part5/README.md#part5---create-a-kubernetes-cron-job-to-run-the-cleaner) - Create a kubernetes cron job to run the cleaner

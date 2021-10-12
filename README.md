# SAFE postgres migrations

This fork of the [SAFE bookstore project](https://github.com/SAFE-Stack/SAFE-BookStore) shows:
* How to run the app on kuberentes
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
```.\build.cmd BundleClient```
1. Build the docker file  
```docker build -t book-store .```
1. Deploy to docker  
```docker run -d -p 80:8085 book-store```   
Open http://localhost:80 to see it running.

## Part1 - Creating a kubernetes deployment

1. Run the image as a kubernetes deployment  
```kubectl apply -f .\singleApp.yaml```  
1. Expose the deployment on a local port
```kubectl expose deployment book-store --type=LoadBalancer```  
Open http://localhost:8085/ to see it running

## Part2 - Add Postgres storage to the app

1. Run paket to install the new `Npgsql` dependency  
```.\.paket\paket install```
1. Create a postgres mount  
```kubectl apply -f .\postgres\storage.yaml```  
1. Create a postgres deployment  
```kubectl apply -f .\postgres\deployment.yaml```  
1. Expose the database
```kubectl expose deployment book-store-db --type=LoadBalancer```  
1. Re-deploy the site  
```.\build.cmd BundleClient; docker build -t book-store .; kubectl rollout restart deployment/book-store```  
and to check on the status:
```kubectl rollout status deployment book-store```

The site will now be failing as it connects to the database, which has no tables.

# Next steps

1. [Part3](https://github.com/ChrisCanCompute/SAFE-BookStore/blob/Part3/README.md#part3---add-postgres-migrations) - Add Postgres migrations
1. [Part4](https://github.com/ChrisCanCompute/SAFE-BookStore/blob/Part4/README.md#part4---create-a-cleaning-app) - Create a cleaning app
1. [Part5](https://github.com/ChrisCanCompute/SAFE-BookStore/blob/Part5/README.md#part5---create-a-kubernetes-cron-job-to-run-the-cleaner) - Create a kubernetes cron job to run the cleaner

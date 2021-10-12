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

## Part3 - Add Postgres migrations

1. Run paket to install the new `Simple.Migrations` dependency  
```.\.paket\paket install```
1. Redploy the app to run the migrations  
```.\build.cmd BundleClient; docker build -t book-store .; kubectl rollout restart deployment/book-store```  

The [site](http://localhost:8085/) now loads, backed by a Postgres database.

## Part4 - Create a cleaning app

1. Build the cleaner app  
```.\build.cmd BuildCleaner```
1. Run the cleaner app  
```dotnet run --project .\src\Cleaner\Cleaner.fsproj "PostgresConnection=Host=localhost;Username=postgresadmin;Password=admin123;Database=postgresdb"```

# Part5 - Create a kubernetes cron job to run the cleaner

1. Bundle the cleaner app  
```.\build.cmd BundleCleaner```
1. Build the cleaner image  
```docker build -t .\src\Cleaner```
1. Create the cron job  
```kubectl apply -f .\src\Cleaner\cleaner.yaml```
1. Check it exists  
```kubectl get cronjob```
1. Do a manual run  
```kubectl create job --from=cronjob/book-store-cleaner clean-now```
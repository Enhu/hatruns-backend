# Hatruns.com-backend

This is the backend side of the hatruns.com website.
 
# Building

In order run the application, you need to install **SQL Server** (the latest version, this will change in the future to PostgreSQL) and **Visual Studio** (preferably the 2022 version).

After cloning the repository, build the solution then open the **Package Manager Console** and select the **hatruns.Database** project on Default Project.

Now run this command:

>ADD-MIGRATION Initial

This will create a Migrations folder in the project which will generate a new instance of the database after running this next command:

>UPDATE-DATABASE

Now you should have a new clean database generated, run the project at the top and open the Swagger website to see if everything is working.

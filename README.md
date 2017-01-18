
Database
--------

The database is created using Code-First technique of the Entity Framework.

1. Configuration String:
-----------------------
The Configuration String for the database is configured in the respective Web.Config and 
App.Config files of Journals.Web:

	<add name="JournalsDB" connectionString="Data Source=localhost;Initial 
	Catalog=JournalsDB;Integrated Security=True;" providerName="System.Data.SqlClient" />

2. Creating UserProfile Tables & Seeding Users:
-----------------

1. Ensure all the NuGet Packages are available and installed:
a. Right-click the solution and select "Manage NuGet Packages for Solution". If any packages are 
required to be installed, it will indicate at the top of the dialog box. "Install" the 
missing packages. (This may take a while.)
2. Build the Solution and run the Journals.Web application. To run the Journals.Web Appliation:
a. Right-click the Journals.Web project and select "Set as StartUp Project"
b. Hit F5 or Right-click Journals.Web and select Debug > Start a new instance

3. Once the command run successfully, this will create database, UserProfile related tables and 
the following users and roles:

| Username | Password | Role  |
----------------------------------
|pappu  | Passw0rd! | Publisher |
|pappy  | Passw0rd! | Subscriber |
|daniel | Passw0rd! | Publisher  |
|andrew | Passw0rd! | Subscriber |
|serge  | Passw0rd! | Subscriber |
|harold | Passw0rd! | Publisher  |

Running the application:
------------------------

In the solution folder, you will find Journals.sln. Opening that in Visual Studio, will open 
the Solution and load all the projects.

1. Ensure all the NuGet Packages are available and installed:
	a. Right-click the solution and select "Manage NuGet Packages for Solution". If any packages 
	are required to be installed, it will indicate at the top of the dialog box. "Install" the 
	missing packages.

2. Perform the steps mentioned in "Database" section of this document, if you haven't already done 
that.

3. To run the Journals.Web Appliation:
	a. Right-click the Journals.Web project and select "Set as StartUp Project"
	b. Hit F5 or Right-click Journals.Web and select Debug > Start a new instance

This will run the Journals.Web application and launch the hompepage. Once on homepage, you can click 
on the "Publisher" section, login as "daniel" and get started.


	
		
	
			
		
		

		
		
 







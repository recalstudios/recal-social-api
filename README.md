# Recal Social API

The API used by [Recal Social](https://social.recalstudios.net/). This project is created using ASP.NET Core 6.

It is maintained and hosted by Recal Studios, so there is no need to download this project - it simply exists for
transparency, documentation and tinkering. Only download and/or use this repository if you know what you are doing. Pull
requests are welcome.

---

### Installation and usage

You can use this project in one of three ways:

1. **Cloning the project**

    You can clone the project and use the provided Dockerfile to build and run the API.

2. **Using the docker image**

    You can also use the official docker image, hosted on the GitHub Container registry. You can find the newest image
under the _Packages_ section on the right.

3. **Running bare-metal**

    You can also clone the project and run it directly bare-metal through the .NET runtime. There are many ways to do
this, be it through an IDE or directly.

We do not provide any binaries, so the project needs to be built before running. We recommend using the docker image, as
this is the easiest method to setup and maintain.

Whatever method you decide to use, you must provide the task with the following environment variables:

```yml
# Common options
#RECAL_SOCIAL_API_PORT: # The port to host the API on. Optional, defaults to 80
DATABASE_CONNECTION_STRING: # A MySQL connection string for the database
ENABLE_MAIL_SERVICE: # Whether to enable the features of the API that integrate with a mail server

# If ENABLE_MAIL_SERVICE is set to false, none of the following variables are to be provided
MAIL_SERVER: # The mail server to use for passphrase resets
#MAIL_SERVER_PORT: # The port to use for the mail server. Optional, defaults to 587
#MAIL_SENDER_NAME: # The name of the sender of system emails. Optional, defaults to "Recal Social"
MAIL_SENDER_EMAIL: # The email address of the sender
MAIL_USERNAME: # The username for the mail server
MAIL_PASSWORD: # The password for the mail server
```

---

### Database

The API must connect to a MySQL database for data storage. A compliant database can be created with SQL commands as
provided in the _database_create.sql_ file, and visually described in the _database_model.mwb_ file.

You can find information about how to format a connection string (for `DATABASE_CONNECTION_STRING`) [here](https://www.connectionstrings.com/mysql/).

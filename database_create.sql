-- Recal Social database creation script
-- This script creates the schema and relevant tables for the Recal Social database structure.

-- Create and use schema
create schema if not exists recal_social_database;
use recal_social_database;

-- Chatrooms table
create table if not exists chatrooms
(
    cid        int auto_increment
        primary key,
    name       varchar(128)                                             not null,
    image      varchar(512) default 'https://via.placeholder.com/50x50' null,
    code       varchar(8)                                               null,
    pass       varchar(128)                                             null,
    lastActive datetime     default current_timestamp()                 null,
    constraint chatrooms_pk_2
        unique (code)
);

-- Users table
create table if not exists users
(
    uid          int auto_increment
        primary key,
    username     varchar(32)                                                 not null,
    passphrase   varchar(128)                                                not null,
    email        varchar(100)                                                not null,
    pfp          varchar(2046) default 'https://via.placeholder.com/100x100' not null,
    access_level int(1)        default 0                                     not null,
    active       int(1)        default 1                                     null,
    constraint users_pk_2
        unique (username),
    constraint users_pk_3
        unique (email)
);

-- Users has chatrooms (many-to-many relational table)
create table if not exists users_chatrooms
(
    users_uid    int not null,
    chatroom_cid int not null,
    constraint users_chatrooms_pk
        primary key (users_uid, chatroom_cid),
    constraint users_chatrooms_chatrooms_cid_fk
        foreign key (chatroom_cid) references chatrooms (cid),
    constraint users_chatrooms_users_uid_fk
        foreign key (users_uid) references users (uid)
);

-- Refresh token table
create table if not exists refreshtoken
(
    refreshTokenId  int auto_increment
        primary key,
    token           varchar(1024) null,
    created         datetime      null,
    revokationDate  datetime      null,
    manuallyRevoked int           null,
    expiresAt       datetime      null,
    replacesId      int           null,
    replacedById    int           null,
    userId          int           not null,
    constraint refreshtoken_users_uid_fk
        foreign key (userId) references users (uid)
);

-- Messages table
create table if not exists messages
(
    id        int auto_increment
        primary key,
    uid       int                                  not null,
    text      varchar(2500)                        null,
    timestamp datetime default current_timestamp() null,
    cid       int                                  not null,
    constraint messages_chatrooms_cid_fk
        foreign key (cid) references chatrooms (cid),
    constraint messages_users_uid_fk
        foreign key (uid) references users (uid)
);

-- Attachments table
create table if not exists attachments
(
    attachment_id int auto_increment
        primary key,
    message_id    int          not null,
    src           varchar(256) not null,
    type          varchar(32)  not null,
    constraint attachments_messages_id_fk
        foreign key (message_id) references messages (id)
);


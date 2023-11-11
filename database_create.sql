-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
-- -----------------------------------------------------
-- Schema recal_social_database
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema recal_social_database
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `recal_social_database` DEFAULT CHARACTER SET utf8 ;
USE `recal_social_database` ;

-- -----------------------------------------------------
-- Table `recal_social_database`.`chatrooms`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `recal_social_database`.`chatrooms` (
  `cid` INT(11) NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(128) NOT NULL,
  `image` VARCHAR(512) NULL DEFAULT 'https://via.placeholder.com/50x50',
  `code` VARCHAR(8) NULL DEFAULT NULL,
  `pass` VARCHAR(128) NULL DEFAULT NULL,
  `lastActive` DATETIME NULL DEFAULT CURRENT_TIMESTAMP(),
  PRIMARY KEY (`cid`),
  UNIQUE INDEX `chatrooms_code_uindex` (`code` ASC) VISIBLE)
ENGINE = InnoDB
AUTO_INCREMENT = 177
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `recal_social_database`.`users`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `recal_social_database`.`users` (
  `uid` INT(11) NOT NULL AUTO_INCREMENT,
  `username` VARCHAR(32) NOT NULL,
  `passphrase` VARCHAR(128) NOT NULL,
  `email` VARCHAR(100) NOT NULL,
  `pfp` VARCHAR(2046) NOT NULL DEFAULT 'https://via.placeholder.com/100x100',
  `access_level` INT(1) NOT NULL DEFAULT 0,
  `active` INT(1) NULL DEFAULT 1,
  PRIMARY KEY (`uid`),
  UNIQUE INDEX `username_UNIQUE` (`username` ASC) VISIBLE,
  UNIQUE INDEX `email_UNIQUE` (`email` ASC) VISIBLE)
ENGINE = InnoDB
AUTO_INCREMENT = 62
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `recal_social_database`.`messages`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `recal_social_database`.`messages` (
  `id` INT(11) NOT NULL AUTO_INCREMENT,
  `uid` INT(11) NOT NULL,
  `text` VARCHAR(2500) NULL DEFAULT NULL,
  `timestamp` DATETIME NULL DEFAULT CURRENT_TIMESTAMP(),
  `cid` INT(11) NOT NULL,
  PRIMARY KEY (`id`, `uid`, `cid`),
  INDEX `fk_messages_users1_idx` (`uid` ASC) VISIBLE,
  INDEX `fk_messages_chatroom1_idx` (`cid` ASC) VISIBLE,
  CONSTRAINT `fk_messages_chatroom1`
    FOREIGN KEY (`cid`)
    REFERENCES `recal_social_database`.`chatrooms` (`cid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_messages_users1`
    FOREIGN KEY (`uid`)
    REFERENCES `recal_social_database`.`users` (`uid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
AUTO_INCREMENT = 1859
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `recal_social_database`.`attachments`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `recal_social_database`.`attachments` (
  `attachment_id` INT(11) NOT NULL AUTO_INCREMENT,
  `message_id` INT(11) NULL DEFAULT NULL,
  `src` VARCHAR(256) NOT NULL,
  `type` VARCHAR(32) NOT NULL,
  PRIMARY KEY (`attachment_id`),
  INDEX `message_id_link` (`message_id` ASC) VISIBLE,
  CONSTRAINT `message_id_link`
    FOREIGN KEY (`message_id`)
    REFERENCES `recal_social_database`.`messages` (`id`))
ENGINE = InnoDB
AUTO_INCREMENT = 2
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `recal_social_database`.`refreshtoken`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `recal_social_database`.`refreshtoken` (
  `refreshTokenId` INT(11) NOT NULL AUTO_INCREMENT,
  `token` VARCHAR(1024) NULL DEFAULT NULL,
  `created` DATETIME NULL DEFAULT NULL,
  `revokationDate` DATETIME NULL DEFAULT NULL,
  `manuallyRevoked` INT(11) NULL DEFAULT NULL,
  `expiresAt` DATETIME NULL DEFAULT NULL,
  `replacesId` INT(11) NULL DEFAULT NULL,
  `replacedById` INT(11) NULL DEFAULT NULL,
  `userId` INT(11) NOT NULL,
  PRIMARY KEY (`refreshTokenId`, `userId`),
  INDEX `fk_refreshtoken_users1_idx` (`userId` ASC) VISIBLE,
  CONSTRAINT `fk_refreshtoken_users1`
    FOREIGN KEY (`userId`)
    REFERENCES `recal_social_database`.`users` (`uid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
AUTO_INCREMENT = 547
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `recal_social_database`.`users_chatrooms`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `recal_social_database`.`users_chatrooms` (
  `users_uid` INT(11) NOT NULL,
  `chatroom_cid` INT(11) NOT NULL,
  PRIMARY KEY (`users_uid`, `chatroom_cid`),
  INDEX `fk_users_has_chatroom_chatroom1_idx` (`chatroom_cid` ASC) VISIBLE,
  INDEX `fk_users_has_chatroom_users1_idx` (`users_uid` ASC) VISIBLE,
  CONSTRAINT `fk_users_has_chatroom_chatroom1`
    FOREIGN KEY (`chatroom_cid`)
    REFERENCES `recal_social_database`.`chatrooms` (`cid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_users_has_chatroom_users1`
    FOREIGN KEY (`users_uid`)
    REFERENCES `recal_social_database`.`users` (`uid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;

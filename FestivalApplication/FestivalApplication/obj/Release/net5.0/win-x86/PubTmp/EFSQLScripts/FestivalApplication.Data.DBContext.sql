CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE TABLE `MusicList` (
        `ID` int NOT NULL AUTO_INCREMENT,
        `ListName` text NULL,
        PRIMARY KEY (`ID`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE TABLE `Stage` (
        `StageID` int NOT NULL AUTO_INCREMENT,
        `StageName` text NULL,
        `StageActive` tinyint(1) NOT NULL,
        PRIMARY KEY (`StageID`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE TABLE `Track` (
        `TrackID` int NOT NULL AUTO_INCREMENT,
        `TrackName` text NULL,
        `TrackSource` text NULL,
        `Length` int NOT NULL,
        PRIMARY KEY (`TrackID`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE TABLE `User` (
        `UserID` int NOT NULL AUTO_INCREMENT,
        `UserName` text NULL,
        `PassWord` text NULL,
        `Role` text NULL,
        PRIMARY KEY (`UserID`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE TABLE `MusicListActivity` (
        `ID` int NOT NULL AUTO_INCREMENT,
        `ListID` int NOT NULL,
        `StageID` int NOT NULL,
        `PreviousSong` int NOT NULL,
        `NextSong` int NOT NULL,
        `NextNextSong` int NOT NULL,
        `Start` datetime NOT NULL,
        `Finish` datetime NOT NULL,
        `MusicListID` int NULL,
        PRIMARY KEY (`ID`),
        CONSTRAINT `FK_MusicListActivity_MusicList_MusicListID` FOREIGN KEY (`MusicListID`) REFERENCES `MusicList` (`ID`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE TABLE `TrackActivity` (
        `ID` int NOT NULL AUTO_INCREMENT,
        `MusicListID` int NOT NULL,
        `TrackID` int NOT NULL,
        `OrderNumber` int NOT NULL,
        PRIMARY KEY (`ID`),
        CONSTRAINT `FK_TrackActivity_MusicList_MusicListID` FOREIGN KEY (`MusicListID`) REFERENCES `MusicList` (`ID`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE TABLE `Authentication` (
        `AuthenticationID` int NOT NULL AUTO_INCREMENT,
        `UserID` int NULL,
        `AuthenticationKey` text NULL,
        `MaxExpiryDate` datetime NOT NULL,
        `CurrentExpiryDate` datetime NOT NULL,
        PRIMARY KEY (`AuthenticationID`),
        CONSTRAINT `FK_Authentication_User_UserID` FOREIGN KEY (`UserID`) REFERENCES `User` (`UserID`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE TABLE `UserActivity` (
        `UserActivityID` int NOT NULL AUTO_INCREMENT,
        `UserID` int NOT NULL,
        `StageID` int NOT NULL,
        `Entry` datetime NOT NULL,
        `Exit` datetime NULL,
        PRIMARY KEY (`UserActivityID`),
        CONSTRAINT `FK_UserActivity_User_UserID` FOREIGN KEY (`UserID`) REFERENCES `User` (`UserID`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE TABLE `Message` (
        `MessageID` int NOT NULL AUTO_INCREMENT,
        `MessageText` text NULL,
        `Timestamp` datetime NOT NULL,
        `UserActivityID` int NULL,
        PRIMARY KEY (`MessageID`),
        CONSTRAINT `FK_Message_UserActivity_UserActivityID` FOREIGN KEY (`UserActivityID`) REFERENCES `UserActivity` (`UserActivityID`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE INDEX `IX_Authentication_UserID` ON `Authentication` (`UserID`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE INDEX `IX_Message_UserActivityID` ON `Message` (`UserActivityID`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE INDEX `IX_MusicListActivity_MusicListID` ON `MusicListActivity` (`MusicListID`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE INDEX `IX_TrackActivity_MusicListID` ON `TrackActivity` (`MusicListID`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    CREATE INDEX `IX_UserActivity_UserID` ON `UserActivity` (`UserID`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20210930132606_eersteMigration')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20210930132606_eersteMigration', '5.0.9');
END;

COMMIT;


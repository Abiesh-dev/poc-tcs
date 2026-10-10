CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT COLLATE NOCASE NOT NULL,
    "Age" INTEGER NOT NULL,
    "City" TEXT NOT NULL,
    "State" TEXT NOT NULL,
    "Pincode" TEXT COLLATE NOCASE NOT NULL,
    "CreatedAtUtc" TEXT NOT NULL,
    "UpdatedAtUtc" TEXT NOT NULL,
    CONSTRAINT "CK_Users_Age" CHECK ([Age] >= 0 AND [Age] <= 120)
);

CREATE UNIQUE INDEX "IX_Users_Name_Pincode" ON "Users" ("Name", "Pincode");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261009150517_InitialCreate', '8.0.8');

COMMIT;


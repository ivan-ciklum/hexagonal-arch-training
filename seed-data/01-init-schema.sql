-- Crear tabla Clubs si no existe
CREATE TABLE IF NOT EXISTS "Clubs" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL
);

-- Crear tabla Subscriptions PRIMERO (antes de Members que la referencia)
CREATE TABLE IF NOT EXISTS "Subscriptions" (
    "Id" UUID PRIMARY KEY,
    "Type" VARCHAR(50) NOT NULL,
    "StartDate" TIMESTAMP NOT NULL,
    "EndDate" TIMESTAMP NOT NULL,
    "Status" VARCHAR(50) NOT NULL
);

-- Crear tabla Members DESPUÉS (porque referencia a Clubs y Subscriptions)
CREATE TABLE IF NOT EXISTS "Members" (
    "Id" UUID PRIMARY KEY,
    "ClubId" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Email" VARCHAR(200) NOT NULL,
    "SubscriptionId" UUID NOT NULL,
    CONSTRAINT "FK_Members_Clubs" FOREIGN KEY ("ClubId") REFERENCES "Clubs"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Members_Subscriptions" FOREIGN KEY ("SubscriptionId") REFERENCES "Subscriptions"("Id") ON DELETE RESTRICT
);

-- Crear índice único para Name + ClubId (business rule)
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Members_Name_ClubId" 
ON "Members" ("Name", "ClubId");
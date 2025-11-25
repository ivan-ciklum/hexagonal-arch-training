-- Insertar datos iniciales de clubes
INSERT INTO "Clubs" ("Id", "Name") 
VALUES 
    ('a1b2c3d4-e5f6-4a5b-8c9d-1e2f3a4b5c6d', 'Club Deportivo Barcelona'),
    ('b2c3d4e5-f6a7-5b6c-9d0e-2f3a4b5c6d7e', 'Club Social Madrid'),
    ('c3d4e5f6-a7b8-6c7d-0e1f-3a4b5c6d7e8f', 'Club de Tenis Valencia'),
    ('d4e5f6a7-b8c9-7d8e-1f2a-4b5c6d7e8f9a', 'Club Nautico Alicante'),
    ('e5f6a7b8-c9d0-8e9f-2a3b-5c6d7e8f9a0b', 'Club de Golf Sevilla')
ON CONFLICT ("Id") DO NOTHING;

-- Mensaje de confirmacion
DO $$
BEGIN
    RAISE NOTICE 'Seed data para Clubs insertado correctamente';
END $$;
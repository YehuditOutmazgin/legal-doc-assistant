-- ============================================
-- Run All Database Scripts
-- Execute this file to setup complete database
-- Using IDENTITY columns (Oracle 12c+)
-- ============================================

PROMPT ============================================
PROMPT Creating Tables...
PROMPT ============================================
@@01_create_tables.sql

PROMPT ============================================
PROMPT Creating Indexes...
PROMPT ============================================
@@02_create_indexes.sql

PROMPT ============================================
PROMPT Inserting Seed Data...
PROMPT ============================================
@@03_seed_data.sql


PROMPT
PROMPT ============================================
PROMPT Database Setup Complete!
PROMPT ============================================
PROMPT Test Users:
PROMPT   admin@legaldoc.com   (ADMIN)
PROMPT   lawyer@legaldoc.com  (LAWYER)
PROMPT   clerk@legaldoc.com   (CLERK)
PROMPT Password for all: Password123!
PROMPT ============================================

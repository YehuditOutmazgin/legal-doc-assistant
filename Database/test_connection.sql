SET SERVEROUTPUT ON;

PROMPT
PROMPT ============================================
PROMPT Database Verification
PROMPT ============================================

SELECT 'Connected as: ' || USER AS STATUS FROM DUAL;

PROMPT
PROMPT Tables:
SELECT table_name FROM user_tables WHERE table_name IN ('USERS', 'CLIENTS', 'TEMPLATES', 'CONTRACTS', 'AUDIT_LOGS') ORDER BY table_name;

PROMPT
PROMPT Data Count:
SELECT 'USERS: ' || COUNT(*) FROM USERS
UNION ALL
SELECT 'CLIENTS: ' || COUNT(*) FROM CLIENTS
UNION ALL
SELECT 'TEMPLATES: ' || COUNT(*) FROM TEMPLATES
UNION ALL
SELECT 'CONTRACTS: ' || COUNT(*) FROM CONTRACTS
UNION ALL
SELECT 'AUDIT_LOGS: ' || COUNT(*) FROM AUDIT_LOGS;

PROMPT
PROMPT Sample Users:
COLUMN ID FORMAT 999
COLUMN EMAIL FORMAT A30
COLUMN ROLE FORMAT A10
SELECT ID, EMAIL, ROLE FROM USERS ORDER BY ID;

PROMPT
PROMPT ============================================
PROMPT Verification Complete!
PROMPT ============================================

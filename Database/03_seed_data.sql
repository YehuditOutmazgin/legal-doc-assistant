-- ============================================
-- Seed Data for Testing
-- Password for all users: "Password123!"
-- Hash generated with BCrypt
-- ============================================

-- Insert Admin User
INSERT INTO USERS (EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, ROLE, CREATED_AT, IS_ACTIVE)
VALUES (
    'admin@legaldoc.com',
    '$2a$11$K2Qz8jZvZ5xJZvZ5xJZvZeO5xJZvZ5xJZvZ5xJZvZ5xJZvZ5xJZvZ',
    'Admin',
    'User',
    'ADMIN',
    CURRENT_TIMESTAMP,
    1
);

-- Insert Lawyer User
INSERT INTO USERS (EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, ROLE, CREATED_AT, IS_ACTIVE)
VALUES (
    'lawyer@legaldoc.com',
    '$2a$11$K2Qz8jZvZ5xJZvZ5xJZvZeO5xJZvZ5xJZvZ5xJZvZ5xJZvZ5xJZvZ',
    'David',
    'Cohen',
    'LAWYER',
    CURRENT_TIMESTAMP,
    1
);

-- Insert Clerk User
INSERT INTO USERS (EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, ROLE, CREATED_AT, IS_ACTIVE)
VALUES (
    'clerk@legaldoc.com',
    '$2a$11$K2Qz8jZvZ5xJZvZ5xJZvZeO5xJZvZ5xJZvZ5xJZvZ5xJZvZ5xJZvZ',
    'Sarah',
    'Levi',
    'CLERK',
    CURRENT_TIMESTAMP,
    1
);

-- Insert Individual Client
INSERT INTO CLIENTS (NAME, TYPE, EMAIL, PHONE, ADDRESS, CREATED_AT, IS_ACTIVE)
VALUES (
    'Yossi Mizrahi',
    'INDIVIDUAL',
    'yossi@example.com',
    '050-1234567',
    'Rothschild 10, Tel Aviv',
    CURRENT_TIMESTAMP,
    1
);

-- Insert Company Client
INSERT INTO CLIENTS (NAME, TYPE, EMAIL, PHONE, ADDRESS, COMPANY_REGISTRATION_NUMBER, CONTACT_PERSON_NAME, CREATED_AT, IS_ACTIVE)
VALUES (
    'Tech Solutions Ltd',
    'COMPANY',
    'info@techsolutions.co.il',
    '03-9876543',
    'HaYarkon 50, Tel Aviv',
    '514567890',
    'Rachel Green',
    CURRENT_TIMESTAMP,
    1
);

-- Insert Template
INSERT INTO TEMPLATES (NAME, DESCRIPTION, CONTENT, CATEGORY, CREATED_BY_USER_ID, CREATED_AT, IS_ACTIVE)
VALUES (
    'Employment Contract',
    'Standard employment contract template',
    'EMPLOYMENT AGREEMENT' || CHR(10) || CHR(10) ||
    'This Employment Agreement is entered into on [DATE] between:' || CHR(10) || CHR(10) ||
    'Employer: [EMPLOYER_NAME]' || CHR(10) ||
    'Employee: [EMPLOYEE_NAME]' || CHR(10) || CHR(10) ||
    'Terms:' || CHR(10) ||
    '1. Position: [POSITION]' || CHR(10) ||
    '2. Salary: [SALARY] per month' || CHR(10) ||
    '3. Start Date: [START_DATE]' || CHR(10) || CHR(10) ||
    'Signatures:' || CHR(10) ||
    '_________________    _________________' || CHR(10) ||
    'Employer             Employee',
    'Employment',
    1,
    CURRENT_TIMESTAMP,
    1
);

-- Insert Draft Contract
INSERT INTO CONTRACTS (TITLE, CONTENT, STATUS, CLIENT_ID, TEMPLATE_ID, CREATED_BY_USER_ID, CREATED_AT)
VALUES (
    'Employment Agreement - Yossi Mizrahi',
    'EMPLOYMENT AGREEMENT' || CHR(10) || CHR(10) ||
    'This Employment Agreement is entered into on 2026-05-01 between:' || CHR(10) || CHR(10) ||
    'Employer: Tech Solutions Ltd' || CHR(10) ||
    'Employee: Yossi Mizrahi' || CHR(10) || CHR(10) ||
    'Terms:' || CHR(10) ||
    '1. Position: Software Developer' || CHR(10) ||
    '2. Salary: 25,000 ILS per month' || CHR(10) ||
    '3. Start Date: 2026-06-01',
    'DRAFT',
    1,
    1,
    2,
    CURRENT_TIMESTAMP
);

-- Insert Audit Log
INSERT INTO AUDIT_LOGS (CONTRACT_ID, USER_ID, ACTION, DETAILS, TIMESTAMP)
VALUES (
    1,
    2,
    'CREATED',
    'Contract created from template',
    CURRENT_TIMESTAMP
);

COMMIT;

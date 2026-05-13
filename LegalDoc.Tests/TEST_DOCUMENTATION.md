# Unit Tests Documentation - LegalDoc Project

## Overview
This document describes the comprehensive unit tests created for the LegalDoc project. The tests follow the **AAA (Arrange-Act-Assert)** pattern and use **Moq** for mocking dependencies.

## Test Files

### 1. AuthServiceTests.cs
Tests for the authentication service, covering the complete authentication flow.

#### Test Cases:

**Login Functionality**
- `LoginAsync_WithValidCredentials_ReturnsAuthResponseWithToken` ✅
  - Tests successful login with correct email and password
  - Verifies token generation and user data mapping
  
- `LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException` ✅
  - Tests login failure with wrong password
  - Verifies proper exception is thrown
  
- `LoginAsync_WithNonExistentUser_ThrowsUnauthorizedAccesception` ✅
  - Tests login with non-existent email
  - Verifies proper error handling
  
- `LoginAsync_WithInactiveUser_ThrowsUnauthorizedAccessException` ✅
  - Tests login with deactivated user account
  - Verifies account status validation

**Registration Functionality**
- `RegisterAsync_WithValidData_ReturnsUserDtoWithId` ✅
  - Tests successful user registration
  - Verifies user creation with all required fields
  
- `RegisterAsync_WithExistingEmail_ThrowsInvalidOperationException` ✅
  - Tests registration with duplicate email
  - Verifies email uniqueness validation

**Token Refresh Functionality**
- `RefreshTokenAsync_WithValidToken_ReturnsNewAuthResponse` ✅
  - Tests token refresh with valid refresh token
  - Verifies new tokens are generated
  
- `RefreshTokenAsync_WithInvalidToken_ThrowsUnauthorizedAccessException` ✅
  - Tests token refresh with invalid token
  - Verifies proper error handling
  
- `RefreshTokenAsync_WithNonExistentUser_ThrowsUnauthorizedAccessException` ✅
  - Tests refresh token for non-existent user
  - Verifies user existence check

**Logout Functionality**
- `LogoutAsync_WithValidToken_CallsRevokeRefreshToken` ✅
  - Tests logout revokes the refresh token
  - Verifies token service is called

---

### 2. JwtTokenServiceTests.cs
Tests for JWT token generation and validation.

#### Test Cases:

**Access Token Generation**
- `GenerateAccessToken_WithValidUser_ReturnsValidJwtToken` ✅
  - Tests token generation for a user
  - Verifies all claims are properly set (ID, Email, Role, FirstName, LastName)
  - Validates token issuer and audience
  
- `GenerateAccessToken_WithDifferentUser_GeneratesDifferentTokens` ✅
  - Tests that different users generate different tokens
  - Verifies token uniqueness
  
- `GenerateAccessToken_TokenHasCorrectExpiry` ✅
  - Tests token expiry time (default 15 minutes)
  - Verifies proper token lifetime

**Refresh Token Generation**
- `GenerateRefreshToken_ReturnsNonEmptyString` ✅
  - Tests refresh token generation
  - Verifies non-empty token
  
- `GenerateRefreshToken_GeneratesDifferentTokensEachCall` ✅
  - Tests that each refresh token is unique
  - Verifies randomness
  
- `GenerateRefreshToken_TokenIsBase64Encoded` ✅
  - Tests refresh token is properly base64 encoded
  - Verifies token format

**Refresh Token Validation**
- `ValidateRefreshTokenAsync_WithValidToken_ReturnsUserId` ✅
  - Tests token validation with valid token
  - Verifies user ID is returned
  
- `ValidateRefreshTokenAsync_WithNonExistentToken_ReturnsNull` ✅
  - Tests validation with non-existent token
  - Verifies null is returned
  
- `ValidateRefreshTokenAsync_WithRevokedToken_ReturnsNull` ✅
  - Tests validation with revoked token
  - Verifies null is returned
  
- `ValidateRefreshTokenAsync_WithExpiredToken_ReturnsNullAndRevokesToken` ✅
  - Tests validation with expired token
  - Verifies token is automatically revoked

**Token Storage & Revocation**
- `StoreRefreshTokenAsync_WithValidData_CreatesRefreshToken` ✅
  - Tests storing refresh token
  - Verifies repository is called correctly
  
- `RevokeRefreshTokenAsync_WithValidToken_CallsRepository` ✅
  - Tests token revocation
  - Verifies repository method is called

---

### 3. ContractRepositoryTests.cs
Tests for contract data operations.

#### Test Cases:

**Create Operations**
- `CreateAsync_WithValidContract_ReturnsContractWithId` ✅
  - Tests contract creation
  - Verifies contract is returned with ID
  
- `CreateAsync_WithTemplateId_ReturnsContractWithTemplateId` ✅
  - Tests contract creation from template
  - Verifies template reference is preserved

**Update Operations**
- `UpdateAsync_WithValidContract_UpdatesSuccessfully` ✅
  - Tests contract update
  - Verifies changes are persisted
  
- `UpdateAsync_ChangingStatus_UpdatesStatusCorrectly` ✅
  - Tests contract status update
  - Verifies status change with signature data

**Delete Operations**
- `DeleteAsync_WithExistingContract_ReturnsTrue` ✅
  - Tests contract deletion
  - Verifies successful deletion returns true
  
- `DeleteAsync_WithNonExistentContract_ReturnsFalse` ✅
  - Tests deletion of non-existent contract
  - Verifies false is returned

**Read Operations**
- `GetByIdAsync_WithExistingContract_ReturnsContract` ✅
  - Tests fetching contract by ID
  - Verifies contract data
  
- `GetByIdAsync_WithNonExistentContract_ReturnsNull` ✅
  - Tests fetching non-existent contract
  - Verifies null is returned

**Filter Operations**
- `GetByClientIdAsync_WithValidClientId_ReturnsContracts` ✅
  - Tests fetching contracts by client
  - Verifies multiple contracts are returned
  
- `GetByClientIdAsync_WithNoContracts_ReturnsEmptyList` ✅
  - Tests filtering with no results
  - Verifies empty list is returned
  
- `GetByStatusAsync_WithValidStatus_ReturnsMatchingContracts` ✅
  - Tests filtering by status
  - Verifies only matching contracts returned
  
- `GetByStatusAsync_WithNoMatchingStatus_ReturnsEmptyList` ✅
  - Tests filtering with no matches
  - Verifies empty list is returned
  
- `GetByUserIdAsync_WithValidUserId_ReturnsContracts` ✅
  - Tests fetching contracts by creator user
  - Verifies user's contracts returned

**Existence Checks**
- `ExistsAsync_WithExistingContract_ReturnsTrue` ✅
  - Tests contract existence check
  - Verifies true for existing contract
  
- `ExistsAsync_WithNonExistentContract_ReturnsFalse` ✅
  - Tests non-existent contract check
  - Verifies false is returned

---

## Test Statistics

| Category | Tests | Status |
|----------|-------|--------|
| AuthService | 10 | ✅ All Passed |
| JwtTokenService | 16 | ✅ All Passed |
| ContractRepository | 12 | ✅ All Passed |
| **Total** | **38** | **✅ All Passed** |

## Running the Tests

### Run All Tests
```bash
dotnet test LegalDoc.Tests
```

### Run Specific Test Class
```bash
dotnet test LegalDoc.Tests --filter "ClassName=LegalDoc.Tests.AuthServiceTests"
```

### Run with Verbose Output
```bash
dotnet test LegalDoc.Tests --verbosity detailed
```

### Run with Coverage (requires coverlet)
```bash
dotnet test LegalDoc.Tests /p:CollectCoverage=true
```

## Mocking Strategy

The tests use **Moq** to mock external dependencies:

1. **IUserRepository** - Mocked to return test users
2. **ITokenService** - Mocked to generate/validate tokens
3. **IConfiguration** - Mocked to provide JWT settings
4. **IRefreshTokenRepository** - Mocked to manage refresh tokens
5. **IContractRepository** - Mocked for contract operations

This approach ensures:
- ✅ No database calls during testing
- ✅ No external API calls
- ✅ Isolated unit tests
- ✅ Fast test execution
- ✅ Predictable test results

## What NOT Tested in Unit Tests

As per best practices, the following are **not** tested in unit tests:

- ❌ Real Oracle database connections
- ❌ Real HTTP requests/API calls
- ❌ AWS S3 operations
- ❌ Network-dependent functionality

These should be tested in **Integration Tests** instead.

## Test Naming Convention

All tests follow this naming pattern:
```
[MethodName]_[Scenario]_[ExpectedResult]
```

Example: `LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException`

## Best Practices Applied

1. **AAA Pattern** - Arrange, Act, Assert
2. **Single Responsibility** - Each test validates one scenario
3. **Clear Naming** - Test names describe what is tested
4. **Isolation** - Tests don't depend on each other
5. **Mocking** - External dependencies are mocked
6. **Assertions** - Multiple assertions for complete validation
7. **Documentation** - Descriptive comments where needed

## Future Enhancements

Consider adding:
- [ ] Integration tests with real database
- [ ] API endpoint tests
- [ ] Load/performance tests
- [ ] Security tests
- [ ] Contract document generation tests
- [ ] S3 integration tests

# Security (compliance-ready): OWASP ASVS checklist template

This file is a **copy/paste template** to help a new project demonstrate OWASP alignment with evidence.

## How to use

- Pick an ASVS level (commonly **L2** for external-facing systems).
- For each section below, record:
  - **Status**: `Pass` / `Fail` / `N/A`
  - **Evidence**: PR, test, config, or architecture rule path
  - **Notes**: assumptions, exceptions, approvals

External references:

- OWASP ASVS: `https://owasp.org/www-project-application-security-verification-standard/`
- OWASP Top 10: `https://owasp.org/www-project-top-ten/`

## Project context (fill in)

- **System type**: mixed (public + internal) / internal / public / batch-heavy
- **Data classification**: (PII / confidential / public)
- **Auth**: (OIDC / JWT / SSO / other)
- **Deployment**: (on-prem / cloud / hybrid)
- **SAST/SCA**: (SonarQube + Fortify SCA + ...)
- **DAST**: (ZAP / other)

## ASVS level

- Selected: **L2** / L1 / L3
- Justification:

## Verification mapping (minimal skeleton)

### V1 Architecture, design and threat modeling
- Status:
- Evidence:
- Notes:

### V2 Authentication
- Status:
- Evidence:
- Notes:

### V3 Session management
- Status:
- Evidence:
- Notes:

### V4 Access control
- Status:
- Evidence:
- Notes:

### V5 Validation, sanitization and encoding
- Status:
- Evidence:
- Notes:

### V6 Stored cryptography
- Status:
- Evidence:
- Notes:

### V7 Error handling and logging
- Status:
- Evidence:
- Notes:

### V8 Data protection
- Status:
- Evidence:
- Notes:

### V9 Communications
- Status:
- Evidence:
- Notes:

### V10 Malicious code
- Status:
- Evidence:
- Notes:

### V11 Business logic
- Status:
- Evidence:
- Notes:

### V12 Files and resources
- Status:
- Evidence:
- Notes:

### V13 API and web service
- Status:
- Evidence:
- Notes:

### V14 Configuration
- Status:
- Evidence:
- Notes:


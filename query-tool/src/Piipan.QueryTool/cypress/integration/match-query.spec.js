describe('query tool match query', () => {
  beforeEach(() => {
    cy.visit('https://localhost:5001');
  })

  it('shows required field errors when form is submitted with no data', () => {
    cy.get('form').submit();

    cy.contains('The First name field is required').should('be.visible');
    cy.contains('The Last name field is required').should('be.visible');
    cy.contains('The Date of birth field is required').should('be.visible');
    cy.contains('The SSN field is required').should('be.visible');
  });

  it("shows formatting error for incorrect SSN", () => {
    cy.get('input[name="Query.SocialSecurityNum"]').type("12345");
    cy.get('form').submit();

    cy.contains('SSN must have the form XXX-XX-XXXX').should('be.visible');
  });

  it("shows proper error for too old dates of birth", () => {
    cy.get('input[name="Query.DateOfBirth"]').type("1899-12-31");
    cy.get('form').submit();

    cy.contains('Date of birth must be between 01-01-1900 and today\'s date').should('be.visible');
  });

  it("shows proper error for non-ascii characters in last name", () => {
    cy.get('input[name="Query.LastName"]').type("garcía");
    // Enter other valid form inputs to isolate expected error
    cy.get('input[name="Query.FirstName"]').type("joe");
    cy.get('input[name="Query.DateOfBirth"]').type("1997-01-01");
    cy.get('input[name="Query.SocialSecurityNum"]').type("550-01-6981");

    cy.get('form').submit();

    cy.contains('Change í in garcía').should('be.visible');
  });

  // it("shows an empty state on successful submission without match", () => {


  //   cy.get('input[name="Query.FirstName"]').type("joe");
  //   cy.get('input[name="Query.LastName"]').type("schmo");
  //   cy.get('input[name="Query.DateOfBirth"]').type("1997-01-01");
  //   cy.get('input[name="Query.SocialSecurityNum"]').type("550-01-6981");

  //   cy.get('form').submit();

  //   cy.contains('No matches found').should('be.visible');
  // });

  // it("shows results table on successful submission with a match", () => {
  //   // TODO: stub out submit request
  //   cy.get('input[name="Query.FirstName"]').type("Theodore");
  //   cy.get('input[name="Query.LastName"]').type("Farrington");
  //   cy.get('input[name="Query.DateOfBirth"]').type("1931-10-13");
  //   cy.get('input[name="Query.SocialSecurityNum"]').type("425-46-5417");

  //   cy.get('form').submit();

  //   cy.contains('Results').should('be.visible');
  //   cy.contains('Case ID').should('be.visible');
  //   cy.contains('Participant ID').should('be.visible');
  //   cy.contains('Benefits end month').should('be.visible');
  //   cy.contains('Recent benefit months').should('be.visible');
  //   cy.contains('Protect location?').should('be.visible');
  // });
})

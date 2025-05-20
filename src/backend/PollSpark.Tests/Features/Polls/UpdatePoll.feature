Feature: Update Poll
    As a user
    I want to update my polls
    So that I can modify their content and options

    Background:
        Given I am an authenticated user
        And I have created a poll with title "Test Poll"
        And the poll has options "Option 1" and "Option 2"

    Scenario: Successfully update a poll
        When I update the poll with new title "Updated Poll"
        And I update the description to "Updated description"
        And I update the options to "New Option 1" and "New Option 2"
        Then the poll should be updated successfully
        And the poll should have the new title "Updated Poll"
        And the poll should have the new description "Updated description"
        And the poll should have the new options "New Option 1" and "New Option 2"

    Scenario: Attempt to update poll when not authenticated
        Given I am not authenticated
        When I try to update the poll
        Then I should receive an error message "User not authenticated"

    Scenario: Attempt to update non-existent poll
        When I try to update a poll with ID "non-existent-id"
        Then I should receive an error message "Poll not found"

    Scenario: Attempt to update another user's poll
        Given there is a poll created by another user
        When I try to update that poll
        Then I should receive an error message "You don't have permission to update this poll" 
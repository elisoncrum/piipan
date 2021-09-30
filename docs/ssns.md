# Testing with Social Security Numbers

As much as possible, we avoid adding to the codebase Social Security Numbers that are potentially in-use. However, testing may require using valid SSN's.

The [test_ssns.csv](./test_ssns.csv) file is a list of SSN's that will [pass system validation](./pprl.md#social-security-number-ssn) but are no longer in use. They were taken from the [Death Master File](https://sortedbybirthdate.com/small_pages/1928/19280103_1000.html) from persons who died prior to the year 2000. Since the Social Security Administration does not reuse an SSN after a person dies, the numbers in this file are suitable for testing.

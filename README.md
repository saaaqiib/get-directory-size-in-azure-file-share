#What it does

When you call this HTTP triggered function app, you pass a query paramater called "user" in the GET call.
The fucntion app takes its value and tries to find a directory with this name inside an azure file share using its access key via environment variables

It then retruns the total size of all items in that directory in MBs as a JSON response.

This is just part of a bigger workflow I set up for a use case. Related reposistory to this one is:
https://github.com/s1mplyfrost/get-username-from-email-after-authentication

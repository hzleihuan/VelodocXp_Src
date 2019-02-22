To run the tests successfully you should only have to change the values in Contants.cs
to match your local configuration and update smtp server settings in App.config.

You may get the following message:
Test Run deployment issue: The location of the file or directory <assembly path> is not trusted. 
There are good chances this is related to the reference to WatiN.Core.dll.
Edit the file poperties from windows explorer and if you see:
This file came from another computer and might be blocked...
Click the Unblock button
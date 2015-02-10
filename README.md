# Nymphicus for Windows
Nymphicus for Windows is a multi protocol social media client (Twitter, App.net, Facebook and QuoteFM) for the Windows desktop (not ModernUI) written in C#

## License
It is licensed under the simplified BSD license

## Getting started
###Libraries
Nymphicus has quite some dependencies to external libraries. Many of them are included as binaries, some are added using Nuget. Sometimes after having checked out the Nuget references do not work correctly - so if you get unknown namespaces add the fitting Nugets, e.g. "Install-package TweetSharp" (maybe after a "Uninstall-package TweetSharp" if you get an "already installed").

### API keys
Of course Nymphicus needs API keys for many services like Twitter, App.net or some external servies (TwitLonger, Pocket, ...) so for all the components you want to use you have to get one on their developer pages. I am sorry that I can't provide my ones as this would violate the permissions bound to the API access in most if not all cases.
I still have (of course) all the valid API keys - so if yu want to create an official release we might work together.

### Setup file
Setup file is created using http://sourceforge.net/projects/nsis/files/NSIS%202/2.46/ 
Install NSIS and then go to bin -> release -> Setup and right click on Setup.nsi and choose to create the installer

## (Very) basic concepts
### Accounts and timeline fetching
The topmost important class in Nymphicus is the Interface called IAccount within the Model folder. All account stuff is done there (this includes authorization, getting timelines and so on) by classes implementing this class. Have a look at Model/AccounAppDotNet.cs for an example (it's cleaner than the much older Twitter one).
Every account has numerous ObservableCollections holding different collections of tweets/posts/statuses/... which are either updated using BackgroundWorker or streaming (Twitter, App.net).
As said look at AccountAppDotNet.cs - it is not to long and shows the concept.

### User interface
I have to say every account type has its own XAML-file for displaying entries (it would make sense to add an abstraction layer here too).
They all can be found in the Controls folder

## Getting help
Feel free to contact me @SvenWal on Twitter or directly sven@tlhan-ghun.de

## More help
... might be added - currently I am putting all my stuff to Open Source so time is even more limited


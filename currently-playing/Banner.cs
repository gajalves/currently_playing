﻿namespace currently_playing;

internal static class Banner
{
    public static void Print()
    {
        Console.WriteLine(@"
          ____                          _   _
         / ___|   _ _ __ _ __ ___ _ __ | |_| |_   _
        | |  | | | | '__| '__/ _ \ '_ \| __| | | | |
        | |__| |_| | |  | | |  __/ | | | |_| | |_| |
         \____\__,_|_|  |_|  \___|_| |_|\__|_|\__, |
                                              |___/
             ____  _             _
            |  _ \| | __ _ _   _(_)_ __   __ _
            | |_) | |/ _` | | | | | '_ \ / _` |
            |  __/| | (_| | |_| | | | | | (_| |
            |_|   |_|\__,_|\__, |_|_| |_|\__, |
                           |___/         |___/               

                            Coded by gajalves
        ");
    }

    public static void Help()
    {
        Console.WriteLine(@"
            Commands:
              -s <service>       Enter configuration mode for a service (spotify or github).
              -id <client_id>    Set the Spotify Client ID.
              -secret <secret>   Set the Spotify Client Secret.
              -auth              Start Spotify OAuth authorization flow.
              -user <username>   Set the GitHub username.
              -token <token>     Set the GitHub token.
              -save              Save the current configuration.
              -r                 Run the application.
              exit               Exit the application.

            Example:
              > -s spotify
              > -id my_client_id
              > -secret my_client_secret
              > -save
              > -auth
              > -s github
              > -user my_github_user
              > -token my_github_token
              > -save
              > -r
              > exit
        ");
    }
}

open System
Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + "/usr/lib")
Environment.GetEnvironmentVariable("DYLD_FALLBACK_LIBRARY_PATH")
#load "NCurses.fs"
open Fncurses



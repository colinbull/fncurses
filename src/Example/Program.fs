// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open Fncurses
open Fncurses.Control
open Fncurses.NCurses

module Example =

    // Book

    let add1 () =
        result {
            do! "Greetings from NCurses!".ToCharArray() 
                |> ResultArray.iter (fun ch ->
                    result { 
                        do! addch ch
                        do! refresh ()
                        do! napms 100
                    })
        }

    let add2 () =
        result {
            let text1 = "Oh give me a clone!\n"
            let text2 = "Yes a clone of my own!"
            do! addstr text1
            do! addstr text2
            do! refresh ()
        }

    let add3 () =
        result {
            let text1 = "Oh give me a clone!\n"
            let text2 = "Yes a clone of my own!"
            do! addstr text1
            do! addstr text2
            do! move 2 0
            do! addstr "With the Y chromosome changed to the X."
            do! refresh ()
        }

    let annoy () =
        result {
            let text = [|"Do"; "you"; "find"; "this"; "silly?"|]
            do! [|0 .. 4|]
                |> ResultArray.iter (fun a ->
                    result {
                        do! [|0 .. 4|]
                            |> ResultArray.iter (fun b ->
                                result {
                                    //if b = a then do! attrset (A_BOLD ||| A_UNDERLINE)
                                    //do! printw "%s" text.[b]
                                    //if b = a then do! attroff (A_BOLD ||| A_UNDERLINE);
                                    do! addch ' '
                                })
                        do! addstr "\b\n"
                    })
            do! refresh ()
        }

    // Book Reference

    let addch () =
        result {
            do! addch 'H'
            do! addch 'i'
            do! addch '!'
            do! refresh ()
        }

    let addchstr () =
        result {
            do! addchstr([|uint32 'H';uint32 'e';uint32 'l';uint32 'l';uint32 'o';0u|])
            do! refresh ()
        }

    let run f =
        result {
            let! win = initscr ()
            do! f ()
            let! ch = getch ()
            return! endwin ()
        }

[<EntryPoint>]
let main argv =
    match Example.run Example.add1 with
    | Success _ -> 0
    | Failure reason -> printfn "%s" reason; 1

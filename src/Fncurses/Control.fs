namespace Fncurses

/// Basic F# Operators. This module is automatically opened in all F# code.
[<AutoOpen>]
module Operators =

    /// <summary>Determines if a reference is a null reference.</summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    [<CompiledName("IsNull")>]
    let inline isNull<'T when 'T : not struct> (arg : 'T) =
        // OPTIMIZE :   Implement with inline IL (ldnull, ldarg.0, ceq). We can't use LanguagePrimitives.PhysicalEquality because it
        //              requires the 'null' constraint which we don't want to require for this function.
        System.Object.ReferenceEquals (null, arg)

    /// <summary>
    /// Determines if a reference is a null reference, and if it is, throws an <see cref="System.ArgumentNullException"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="paramName">The name of the parameter that causes this exception.</param>
    /// <param name="arg">The reference to check.</param>
    [<CompiledName("CheckNonNull")>]
    let inline checkNonNull<'T when 'T : not struct> paramName (arg : 'T) =
        if isNull arg then
            if System.String.IsNullOrWhiteSpace paramName then
                raise <| System.ArgumentNullException ()
            else
                raise <| System.ArgumentNullException paramName


module Control = 

    type Result<'TSuccess,'TFailure> = 
    | Success of 'TSuccess
    | Failure of 'TFailure

    /// <summary>
    /// </summary>
    [<Sealed>]
    type ResultBuilder () =
        /// The zero value for this builder never changes and is immutable,
        /// so create and reuse a single instance of it to avoid unnecessary allocations.
        static let zero = Success ()

        // 'T -> M<'T>
        member inline __.Return value : Result<'T, 'Error> =
            Success value

    #if FX_ATLEAST_FSHARP_3_0
        // Error operation. Similar to the Return method ('return'), but used for returning an error value.
        [<CustomOperation("error")>]
        member inline __.Error value : Result<'T, 'Error> =
            Failure value
    #endif

        // M<'T> -> M<'T>
        member inline __.ReturnFrom (m : Result<'T, 'Error>) =
            m

        // unit -> M<'T>
        member __.Zero () : Result<unit, 'Error> =
            zero

        // (unit -> M<'T>) -> M<'T>
        member __.Delay (generator : unit -> Result<'T, 'Error>) : Result<'T, 'Error> =
            generator ()

        // M<'T> -> M<'T> -> M<'T>
        // or
        // M<unit> -> M<'T> -> M<'T>
        member inline __.Combine (r1, r2) : Result<'T, 'Error> =
            match r1 with
            | Failure error ->
                Failure error
            | Success () ->
                r2

        // M<'T> * ('T -> M<'U>) -> M<'U>
        member inline __.Bind (value, binder : 'T -> Result<'U, 'Error>) : Result<'U, 'Error> =
            match value with
            | Failure error ->
                Failure error
            | Success x ->
                binder x

        // M<'T> * (exn -> M<'T>) -> M<'T>
        member inline __.TryWith (body : 'T -> Result<'U, 'Error>, handler) =
            fun value ->
                try body value
                with ex ->
                    handler ex

        // M<'T> * (unit -> unit) -> M<'T>
        member inline __.TryFinally (body : 'T -> Result<'U, 'Error>, handler) =
            fun value ->
                try body value
                finally
                    handler ()

        // 'T * ('T -> M<'U>) -> M<'U> when 'T :> IDisposable
        member this.Using (resource : ('T :> System.IDisposable), body : _ -> Result<_,_>)
            : Result<'U, 'Error> =
            try body resource
            finally
                if not <| isNull (box resource) then
                    resource.Dispose ()

        // (unit -> bool) * M<'T> -> M<'T>
        member this.While (guard, body : Result<unit, 'Error>) : Result<_,_> =
            if guard () then
                // OPTIMIZE : This could be simplified so we don't need to make calls to Bind and While.
                this.Bind (body, (fun () -> this.While (guard, body)))
            else
                this.Zero ()

        // seq<'T> * ('T -> M<'U>) -> M<'U>
        // or
        // seq<'T> * ('T -> M<'U>) -> seq<M<'U>>
        member this.For (sequence : seq<_>, body : 'T -> Result<unit, 'Error>) =
            // OPTIMIZE : This could be simplified so we don't need to make calls to Using, While, Delay.
            this.Using (sequence.GetEnumerator (), fun enum ->
                this.While (
                    enum.MoveNext,
                    this.Delay (fun () ->
                        body enum.Current)))

    [<CompiledName("Result")>]
    let result = ResultBuilder ()
 
    /// <summary>
    /// </summary>
    /// <typeparam name="State"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Error"></typeparam>
    type StatefulResultFunc<'State, 'TSuccess, 'TFailure> =
        'State -> Result<'TSuccess, 'TFailure> * 'State

    [<Sealed>]
    type StatefulResultBuilder () =
        // 'T -> M<'T>
        member __.Return value
            : StatefulResultFunc<'State, 'T, 'Error> =
            fun state ->
            (Success value), state
    
        // M<'T> -> M<'T>
        member __.ReturnFrom (func)
            : StatefulResultFunc<_,_,_> =
            func
    
        // unit -> M<'T>
        member inline this.Zero ()
            : StatefulResultFunc<'State, unit, 'Error> =
            this.Return ()
    
        // (unit -> M<'T>) -> M<'T>
        member this.Delay (generator : unit -> StatefulResultFunc<_,_,_>)
            : StatefulResultFunc<'State, 'T, 'Error> =
            fun state -> generator () state
    
        // M<'T> * ('T -> M<'U>) -> M<'U>
        member __.Bind (computation : StatefulResultFunc<_,_,_>, binder : 'T -> StatefulResultFunc<_,_,_>)
            : StatefulResultFunc<'State, 'U, 'Error> =
            fun state ->
            match computation state with
            | (Success value), state ->
                binder value state
            | (Failure error), state ->
                (Failure error), state    
            
        // M<'T> -> M<'T> -> M<'T>
        // or
        // M<unit> -> M<'T> -> M<'T>
        member this.Combine (r1 : StatefulResultFunc<_,_,_>, r2 : StatefulResultFunc<_,_,_>)
            : StatefulResultFunc<'State, 'T, 'Error> =
            this.Bind (r1, (fun () -> r2))
    
        // M<'T> * (exn -> M<'T>) -> M<'T>
        member __.TryWith (body : StatefulResultFunc<_,_,_>, handler : exn -> StatefulResultFunc<_,_,_>)
            : StatefulResultFunc<'State, 'T, 'Error> =
            fun state ->
            try body state
            with ex ->
                handler ex state
    
        // M<'T> * (unit -> unit) -> M<'T>
        member __.TryFinally (body : StatefulResultFunc<_,_,_>, handler)
            : StatefulResultFunc<'State, 'T, 'Error> =
            fun state ->
            try body state
            finally
                handler ()
    
        // 'T * ('T -> M<'U>) -> M<'U> when 'T :> IDisposable
        member this.Using (resource : ('T :> System.IDisposable), body : 'T -> StatefulResultFunc<_,_,_>)
            : StatefulResultFunc<'State, 'U, 'Error> =
            fun state ->
            try
                body resource state
            finally
                if not <| isNull (box resource) then
                    resource.Dispose ()
    
        // (unit -> bool) * M<'T> -> M<'T>
        member this.While (guard, body : StatefulResultFunc<_,_,_>)
            : StatefulResultFunc<'State, _, 'Error> =
            if guard () then
                this.Bind (body, (fun () -> this.While (guard, body)))
            else
                this.Zero ()
    
        // seq<'T> * ('T -> M<'U>) -> M<'U>
        // or
        // seq<'T> * ('T -> M<'U>) -> seq<M<'U>>
        member this.For (sequence : seq<_>, body : 'T -> StatefulResultFunc<_,_,_>)
            : StatefulResultFunc<'State, _, 'Error> =
            this.Using (sequence.GetEnumerator (),
                (fun enum ->
                    this.While (
                        enum.MoveNext,
                        this.Delay (fun () ->
                            body enum.Current))))

    [<CompiledName("StatefulResult")>]
    let statefulResult = StatefulResultBuilder ()

module ResultArray =

    open Control
    open Microsoft.FSharp.Control
    open OptimizedClosures

    [<CompiledName("Map")>]
    let map (mapping : 'T -> Result<'U, 'Error>) (array : 'T[]) =
        // Preconditions
        checkNonNull "array" array

        let len = array.Length
        let results = Array.zeroCreate len

        let mutable index = 0
        let mutable error = None

        while index < len && Option.isNone error do
            match mapping array.[index] with
            | Failure err ->
                error <- Some err
            | Success result ->
                results.[index] <- result
                index <- index + 1
            
        // If the error was set, return it; otherwise, return the array of results.
        match error with
        | Some error ->
            Success error
        | None ->
            Failure results

    //
    [<CompiledName("MapIndexed")>]
    let mapi (mapping : int -> 'T -> Result<'U, 'Error>) (array : 'T[]) =
        // Preconditions
        checkNonNull "array" array

        let mapping = FSharpFunc<_,_,_>.Adapt mapping
        let len = array.Length
        let results = Array.zeroCreate len

        let mutable index = 0
        let mutable error = None

        while index < len && Option.isNone error do
            match mapping.Invoke (index, array.[index]) with
            | Failure err ->
                error <- Some err
            | Success result ->
                results.[index] <- result
                index <- index + 1
            
        // If the error was set, return it; otherwise, return the array of results.
        match error with
        | Some error ->
            Failure error
        | None ->
            Success results

    //
    [<CompiledName("Map2")>]
    let map2 (mapping : 'T1 -> 'T2 -> Result<'U, 'Error>) (array1 : 'T1[]) (array2 : 'T2[]) =
        // Preconditions
        checkNonNull "array1" array1
        checkNonNull "array2" array2

        let len = array1.Length
        if array2.Length <> len then
            invalidArg "array2" "The arrays have differing lengths."

        let mapping = FSharpFunc<_,_,_>.Adapt mapping
        let results = Array.zeroCreate len

        let mutable index = 0
        let mutable error = None

        while index < len && Option.isNone error do
            match mapping.Invoke (array1.[index], array2.[index]) with
            | Failure err ->
                error <- Some err
            | Success result ->
                results.[index] <- result
                index <- index + 1                
            
        // If the error was set, return it; otherwise, return the array of results.
        match error with
        | Some error ->
            Failure error
        | None ->
            Success results

    //
    [<CompiledName("Fold")>]
    let fold (folder : 'State -> 'T -> Result<'State, 'Error>) (state : 'State) (array : 'T[]) =
        // Preconditions
        checkNonNull "array" array

        let folder = FSharpFunc<_,_,_>.Adapt folder
        let len = array.Length
        let mutable state = state

        let mutable index = 0
        let mutable error = None

        while index < len && Option.isNone error do
            match folder.Invoke (state, array.[index]) with
            | Failure err ->
                error <- Some err
            | Success newState ->
                state <- newState
                index <- index + 1
            
        // If the error was set, return it; otherwise, return the final state.
        match error with
        | Some error ->
            Failure error
        | None ->
            Success state

    //
    [<CompiledName("FoldIndexed")>]
    let foldi (folder : int -> 'State -> 'T -> Result<'State, 'Error>) (state : 'State) (array : 'T[]) =
        // Preconditions
        checkNonNull "array" array

        let folder = FSharpFunc<_,_,_,_>.Adapt folder
        let len = array.Length
        let mutable state = state

        let mutable index = 0
        let mutable error = None

        while index < len && Option.isNone error do
            match folder.Invoke (index, state, array.[index]) with
            | Failure err ->
                error <- Some err
            | Success newState ->
                state <- newState
                index <- index + 1
            
        // If the error was set, return it; otherwise, return the final state.
        match error with
        | Some error ->
            Failure error
        | None ->
            Success state

    //
    [<CompiledName("Init")>]
    let init (count : int) (initializer : int -> Result<'T, 'Error>) =
        // Preconditions
        if count < 0 then invalidArg "count" "The count cannot be negative."

        let results = Array.zeroCreate count
        let mutable currentIndex = 0
        let mutable error = None

        while currentIndex < count && Option.isNone error do
            match initializer currentIndex with
            | Failure err ->
                error <- Some err

            | Success value ->
                results.[currentIndex] <- value
                currentIndex <- currentIndex + 1

        // If the error is set, return it; otherwise return the initialized array.
        match error with
        | None ->
            Success results
        | Some error ->
            Failure error

    //
    [<CompiledName("Iterate")>]
    let iter (action : 'T -> Result<unit, 'Error>) (array : 'T[]) =
        // Preconditions
        checkNonNull "array" array

        let len = array.Length
        let mutable index = 0
        let mutable error = None

        while index < len && Option.isNone error do
            match action array.[index] with
            | Failure err ->
                error <- Some err
            | Success () ->
                index <- index + 1
            
        // If the error was set, return it.
        match error with
        | Some error ->
            Failure error
        | None ->
            Success ()

    //
    [<CompiledName("IterateIndexed")>]
    let iteri (action : int -> 'T -> Result<unit, 'Error>) (array : 'T[]) =
        // Preconditions
        checkNonNull "array" array

        let action = FSharpFunc<_,_,_>.Adapt action
        let len = array.Length

        let mutable index = 0
        let mutable error = None

        while index < len && Option.isNone error do
            match action.Invoke (index, array.[index]) with
            | Failure err ->
                error <- Some err
            | Success () ->
                index <- index + 1
            
        // If the error was set, return it.
        match error with
        | Some error ->
            Failure error
        | None ->
            Success ()

    //
    [<CompiledName("Reduce")>]
    let reduce (reduction : 'T -> 'T -> Result<'T, 'Error>) (array : 'T[]) =
        // Preconditions
        checkNonNull "array" array
        if Array.isEmpty array then
            invalidArg "array" "The array is empty."

        let reduction = FSharpFunc<_,_,_>.Adapt reduction
        let len = array.Length

        let mutable state = array.[0]   // The first (0-th) element is the initial state.
        let mutable index = 1   // Start at the *second* element (index = 1)
        let mutable error = None

        while index < len && Option.isNone error do
            match reduction.Invoke (state, array.[index]) with
            | Failure err ->
                error <- Some err
            | Success newState ->
                state <- newState
                index <- index + 1
            
        // If the error was set, return it.
        match error with
        | Some error ->
            Failure error
        | None ->
            Success state
        

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Result =

    open Control

    /// <summary>Does the Result value represent a result value?</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("IsResult")>]
    let inline isResult (value : Result<'T, 'Error>) : bool =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success _ -> true
        | Failure _ -> false

    /// <summary>Does the Result value represent an error value?</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("IsError")>]
    let inline isError (value : Result<'T, 'Error>) : bool =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success _ -> false
        | Failure _ -> true

    /// <summary>Gets the result value associated with the Result.</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("Get")>]
    let get (value : Result<'T, 'Error>) =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success result ->
            result
        | Failure _ ->
            invalidArg "value" "Cannot get the result because the Result`2 instance is an error value."

    /// <summary>Gets the error value associated with the Result.</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("GetError")>]
    let getError (value : Result<'T, 'Error>) =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success _ ->
            invalidArg "value" "Cannot get the error because the Result`2 instance is a result value."
        | Failure error ->
            error

    /// <summary>Creates a Result from a result value.</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("Result")>]
    let inline result value : Result<'T, 'Error> =
        Success value

    /// <summary>Creates a Result from an error value.</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("Error")>]
    let inline error value : Result<'T, 'Error> =
        Failure value

    /// <summary>
    /// Creates a Result representing an error value. The error value in the Result is the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns></returns>
    [<CompiledName("FailWith")>]
    let inline failwith message : Result<'T, string> =
        Failure message

    /// <summary>
    /// Creates a Result representing an error value. The error value in the Result is the specified formatted error message.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    [<CompiledName("PrintFormatToStringThenFail")>]
    let inline failwithf (format : Printf.StringFormat<'T, Result<'U, string>>) =
        Printf.ksprintf failwith format

    /// <summary></summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("OfOption")>]
    let ofOption (value : 'T option) : Result<'T, unit> =
        match value with
        | Some result ->
            Success result
        | None ->
            Failure ()

    /// <summary></summary>
    /// <param name="errorValue"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    // TODO :   Rename this to 'ofOptionDefault' or 'ofOptionWithDefault'.
    //          The "With" suffix should be reserved for higher-order functions. 
    [<CompiledName("OfOptionWith")>]
    let ofOptionWith (errorValue : 'Error) (value : 'T option) : Result<'T, 'Error> =
        match value with
        | Some result ->
            Success result
        | None ->
            Failure errorValue

    /// <summary></summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("ToOption")>]
    let toOption (value : Result<'T, 'Error>) : 'T option =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success result ->
            Some result
        | Failure _ ->
            None

    /// <summary>
    /// When the choice value is <c>Success(x)</c>, returns <c>Success (f x)</c>.
    /// Otherwise, when the choice value is <c>Failure(x)</c>, returns <c>Failure(x)</c>. 
    /// </summary>
    /// <param name="mapping"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("Map")>]
    let map (mapping : 'T -> 'U) (value : Result<'T, 'Error>) =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success result ->
            Success (mapping result)
        | Failure error ->
            Failure error

    /// <summary>
    /// Applies the specified mapping function to a choice value representing an error value (Failure). If the choice
    /// value represents a result value (Success), the result value is passed through without modification.
    /// </summary>
    /// <param name="mapping"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("MapError")>]
    let mapError (mapping : 'Error1 -> 'Error2) (value : Result<'T, 'Error1>) =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success result ->
            Success result
        | Failure error ->
            Failure (mapping error)

    /// <summary>
    /// Applies the specified binding function to a choice value representing a result value (Success). If the choice
    /// value represents an error value (Failure), the error value is passed through without modification.
    /// </summary>
    /// <param name="binding"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("Bind")>]
    let bind (binding : 'T -> Result<'U, 'Error>) value =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success result ->
            binding result
        | Failure error ->
            Failure error

    /// <summary>
    /// Applies the specified binding function to a choice value representing a pair of result values (Success).
    /// If the first component of the pair represents an error value, the error is passed through without modification;
    /// otherwise, if the second component of the pair represents an error value, the error is passed through without
    /// modification; otherwise, both components represent result values, which are applied to the specified binding function.
    /// </summary>
    /// <param name="binding"></param>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    [<CompiledName("Bind2")>]
    let bind2 (binding : 'T -> 'U -> Result<'V, 'Error>) value1 value2 =
        // Preconditions
        checkNonNull "value1" value1
        checkNonNull "value2" value2

        match value1, value2 with
        | Success result1, Success result2 ->
            binding result1 result2
        | Success _, Failure error
        | Failure error, _ ->
            Failure error

    /// <summary></summary>
    /// <param name="predicate"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("Exists")>]
    let exists (predicate : 'T -> bool) (value : Result<'T, 'Error>) : bool =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success result ->
            predicate result
        | Failure _ ->
            false

    /// <summary></summary>
    /// <param name="predicate"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("Forall")>]
    let forall (predicate : 'T -> bool) (value : Result<'T, 'Error>) : bool =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success result ->
            predicate result
        | Failure _ ->
            true

    /// <summary></summary>
    /// <param name="folder"></param>
    /// <param name="state"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("Fold")>]
    let fold (folder : 'State -> 'T -> 'State) (state : 'State) (value : Result<'T, 'Error>) : 'State =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success result ->
            folder state result
        | Failure _ ->
            state

    /// <summary></summary>
    /// <param name="folder"></param>
    /// <param name="value"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    [<CompiledName("FoldBack")>]
    let foldBack (folder : 'T -> 'State -> 'State) (value : Result<'T, 'Error>) (state : 'State) : 'State =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success result ->
            folder result state
        | Failure _ ->
            state

    /// <summary></summary>
    /// <param name="action"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("Iterate")>]
    let iter (action : 'T -> unit) (value : Result<'T, 'Error>) : unit =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Failure _ -> ()
        | Success result ->
            action result

    /// <summary></summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("BindOrRaise")>]
    let inline bindOrRaise (value : Result<'T, #exn>) : 'T =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success result ->
            result
        | Failure ex ->
            raise ex

    /// <summary></summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [<CompiledName("BindOrFail")>]
    let inline bindOrFail (value : Result<'T, string>) : 'T =
        // Preconditions
        checkNonNull "value" value

        match value with
        | Success result ->
            result
        | Failure msg ->
            raise <| exn msg

    /// <summary></summary>
    /// <param name="generator"></param>
    /// <returns></returns>
    [<CompiledName("Attempt")>]
    let attempt generator : Result<'T, _> =
        try Success <| generator ()
        with ex -> Failure ex

    /// <summary>
    /// Composes two functions designed for use with the 'result' workflow.
    /// This function is analagous to the F# (&gt;&gt;) operator.
    /// </summary>
    /// <param name="f"></param>
    /// <param name="g"></param>
    /// <returns></returns>
    [<CompiledName("Compose")>]
    let compose (f : 'T -> Result<'U, 'Error>) (g : 'U -> Result<'V, 'Error>) =
        f >> (bind g)

    /// <summary>
    /// Composes two functions designed for use with the 'result' workflow.
    /// This function is analagous to the F# (&lt;&lt;) operator.
    /// </summary>
    /// <param name="f"></param>
    /// <param name="g"></param>
    /// <returns></returns>
    [<CompiledName("ComposeBack")>]
    let composeBack (f : 'U -> Result<'V, 'Error>) (g : 'T -> Result<'U, 'Error>) =
        g >> (bind f)

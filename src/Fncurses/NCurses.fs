
namespace Fncurses

module NCurses = 

    open Control
    open System
    open System.Runtime.InteropServices    


    [<AutoOpen>]
    module Attributes =

        let private NCURSES_ATTR_SHIFT = 8
        let private NCURSES_BITS(mask,shift) = mask <<< (shift + NCURSES_ATTR_SHIFT)

        let A_NORMAL =     1u - 1u
        let A_ATTRIBUTES = NCURSES_BITS(~~~(1u - 1u),0)
        let A_CHARTEXT =   (NCURSES_BITS(1u,0) - 1u)
        let A_COLOR =      NCURSES_BITS((1u <<< 8) - 1u,0)
        let A_STANDOUT =   NCURSES_BITS(1u, 8)
        let A_UNDERLINE =  NCURSES_BITS(1u, 9)
        let A_REVERSE =    NCURSES_BITS(1u,10)
        let A_BLINK =      NCURSES_BITS(1u,11)
        let A_DIM =        NCURSES_BITS(1u,12)
        let A_BOLD =       NCURSES_BITS(1u,13)
        let A_ALTCHARSET = NCURSES_BITS(1u,14)
        let A_INVIS =      NCURSES_BITS(1u,15)
        let A_PROTECT =    NCURSES_BITS(1u,16)
        let A_HORIZONTAL = NCURSES_BITS(1u,17)
        let A_LEFT =       NCURSES_BITS(1u,18)
        let A_LOW =        NCURSES_BITS(1u,19)
        let A_RIGHT =      NCURSES_BITS(1u,20)
        let A_TOP =        NCURSES_BITS(1u,21)
        let A_VERTICAL =   NCURSES_BITS(1u,22)


    module Imported =
        
        // initscr

        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern IntPtr initscr();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int endwin();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern Boolean isendwin();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern IntPtr newterm(IntPtr ``type``, IntPtr outfd, IntPtr infd);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern IntPtr set_term(IntPtr ``new``);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern IntPtr delscreen(IntPtr screen);

        // addch

        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int addch(uint32 ch);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int waddch(IntPtr win, uint32 ch);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvaddch(int y, int x, uint32 ch);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvwaddch(IntPtr win, int y, int x, uint32 ch);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int echochar(uint32 ch);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wechochar(IntPtr win, uint32 ch);
    
        // getch

        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
        extern int getch();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
        extern int wgetch(IntPtr win);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
        extern int mvgetch(int y, int x);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
        extern int mvwgetch(IntPtr win, int y, int x);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
        extern int ungetch(int ch);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int has_key(int ch);
            
        // refresh

        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int refresh();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wrefresh(IntPtr win);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wnoutrefresh(IntPtr win);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int doupdate();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int redrawwin(IntPtr win);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wredrawln(IntPtr win, int beg_line, int num_lines);

        // addchstr

        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int addchstr(uint32[] chstr);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int addchnstr(uint32[] chstr, int n);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int waddchstr(IntPtr win, uint32[] chstr);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int waddchnstr(IntPtr win, uint32[] chstr, int n);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvaddchstr(int y, int x, uint32[] chstr);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvaddchnstr(int y, int x, uint32[] chstr, int n);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvwaddchstr(IntPtr win, int y, int x, uint32[] chstr);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvwaddchnstr(IntPtr win, int y, int x, uint32[] chstr, int n);

        // addstr

        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int addstr(string str);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int addnstr(string str, int n);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int waddstr(IntPtr win, string str);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int waddnstr(IntPtr win, string str, int n);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvaddstr(int y, int x, string str);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvaddnstr(int y, int x, string str, int n);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvwaddstr(IntPtr win, int y, int x, string str);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvwaddnstr(IntPtr win, int y, int x, string str, int n);

        // def_prog_mode

        type RipOffLineFunInt = delegate of (IntPtr * int) -> int

        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int def_prog_mode();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int def_shell_mode();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int reset_prog_mode();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int reset_shell_mode();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int resetty();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int savetty();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern void getsyx(int& y, int& x);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern void setsyx(int y, int x);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int ripoffline(int line, RipOffLineFunInt init);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int curs_set(int visibility);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int napms(int ms);

        // move

        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int move(int y, int x);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wmove(IntPtr win, int y, int x);

        // attroff

        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int attroff(int attrs);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wattroff(IntPtr win, int attrs);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int attron(int attrs);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wattron(IntPtr win, int attrs);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int attrset(int attrs);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wattrset(IntPtr win, int attrs);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int color_set(int16 color_pair_number, IntPtr opts);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wcolor_set(IntPtr win, int16 color_pair_number, IntPtr opts);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int standend();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wstandend(IntPtr win);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int standout();
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wstandout(IntPtr win);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int attr_get(uint32& attrs, int16& pair, IntPtr opts);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wattr_get(IntPtr win, uint32& attrs, int16& pair, IntPtr opts);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int attr_off(uint32 attrs, IntPtr opts);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wattr_off(IntPtr win, uint32 attrs, IntPtr   opts);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int attr_on(uint32 attrs, IntPtr opts);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wattr_on(IntPtr win, uint32 attrs, IntPtr opts);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int attr_set(uint32 attrs, int16 pair, IntPtr opts);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wattr_set(IntPtr win, uint32 attrs, int16 pair, IntPtr opts);
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int chgat(int n, uint32 attr, int16 color, IntPtr opts)
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int wchgat(IntPtr win, int n, uint32 attr, int16 color, IntPtr opts)
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvchgat(int y, int x, int n, uint32 attr, int16 color, IntPtr opts)
        [<DllImport("libncurses", CallingConvention = CallingConvention.Cdecl)>]
        extern int mvwchgat(IntPtr win, int y, int x, int n, uint32 attr, int16 color, IntPtr opts)


    let verifyIntPtr fname result =
        if result = IntPtr.Zero 
        then Failure (sprintf "%s returned NULL" fname)
        else Success result

    let verifyInt fname result = 
        if result = -1
        then Failure (sprintf "%s returned ERR" fname)
        else Success result

    let verify fname result = 
        if result = -1
        then Failure (sprintf "%s returned ERR" fname)
        else Success ()

    /// Initializes curses mode for single terminal applications.  
    /// Returns a pointer to stdscr.
    let initscr () =
        Imported.initscr() |> verifyIntPtr "initscr"

    /// An application should call endwin before exiting curses mode.
    let endwin () =
        Imported.endwin() |> verifyInt "endwin"

    /// Returns true if endwin has been called without any subsequent 
    /// calls to wrefresh.
    let isendwin () =
        Imported.isendwin()

    /// Initializes curses mode for multiple terminal applications.
    let newterm ``type`` outfd infd =
        Imported.newterm(``type``, outfd, infd) |> verifyIntPtr "newterm"

    /// Switch to the screen. Returns the previous terminal.
    let set_term ``new`` =
        Imported.set_term(``new``) |> verifyIntPtr "set_term"

    /// Frees storage associated with the screen.
    let delscreen screen =
        Imported.delscreen(screen) |> verifyIntPtr "delscreen"

    /// Adds a character at the current window position. 
    let inline addch ch =
        Imported.addch(uint32 ch) |> verify "addch"
    
    /// Adds a character at the current window position. 
    let inline waddch win ch =
        Imported.waddch(win, uint32 ch) |> verify "waddch"

    /// Adds a character at the current window position. 
    let inline mvaddch win y x ch =
        Imported.mvaddch(y, x, uint32 ch) |> verify "mvaddch"

    /// Adds a character at the current window position. 
    let inline mvwaddch win y x ch =
        Imported.mvwaddch(win, y, x, uint32 ch) |> verify "mvwaddch"

    /// Adds a character at the current window position and refreshes. 
    let inline echochar ch =
        Imported.echochar(uint32 ch) |> verify "echochar"

    /// Adds a character at the current window position and refreshes. 
    let inline wechochar win ch =
        Imported.wechochar(win, uint32 ch) |> verify "wechochar"

    /// Gets characters from curses terminal keyboard.
    let getch () =
        Imported.getch() |> verifyInt "wgetch"

    /// Gets characters from curses terminal keyboard.
    let wgetch win =
        Imported.wgetch(win) |> verifyInt "wgetch"

    /// Gets characters from curses terminal keyboard.
    let mvwgetch win y x =
        Imported.mvwgetch(win, y, x) |> verifyInt "mvwgetch"

    /// Places a character back onto the input queue.
    let inline ungetch ch =
        Imported.ungetch(int ch) |> verifyInt "ungetch"

    /// Returns true or false if the key value is recognized by the current terminal.
    let inline has_key ch =
        Imported.has_key(int ch) |> verifyInt "has_key"

    /// Applies updates to the terminal.
    let refresh () =
        Imported.refresh() |> verify "wrefresh"

    /// Applies updates to the terminal.
    let wrefresh win =
        Imported.wrefresh(win) |> verify "wrefresh"

    /// Applies updates to the terminal.
    let wnoutrefresh win =
        Imported.wnoutrefresh(win) |> verify "wnoutrefresh"

    /// Applies updates to the terminal.
    let doupdate () =
        Imported.doupdate() |> verify "doupdate"

    /// Redraws the terminal.
    let redrawwin win =
        Imported.redrawwin(win) |> verify "redrawwin"

    /// Redraws the terminal.
    let wredrawln win beg_line num_lines =
        Imported.wredrawln(win, beg_line, num_lines) |> verify "wredrawln"

    /// Copies a character string to the terminal.
    let addchstr chstr =
        Imported.addchstr(chstr) |> verify "addchstr"

    /// Copies a character string to the terminal.
    let addchnstr chstr n =
        Imported.addchnstr(chstr, n) |> verify "addchnstr"

    /// Copies a character string to the terminal.
    let waddchstr win chstr =
        Imported.waddchstr(win, chstr) |> verify "waddchstr"

    /// Copies a character string to the terminal.
    let waddchnstr win chstr n =
        Imported.waddchnstr(win, chstr, n) |> verify "waddchnstr"

    /// Copies a character string to the terminal.
    let mvaddchstr y x chstr =
        Imported.mvaddchstr(y, x, chstr) |> verify "mvaddchstr"

    /// Copies a character string to the terminal.
    let mvaddchnstr y x chstr n =
        Imported.mvaddchnstr(y, x, chstr, n) |> verify "mvaddchnstr"

    /// Copies a character string to the terminal.
    let mvwaddchstr win y x chstr =
        Imported.mvwaddchstr(win, y, x, chstr) |> verify "mvwaddchstr"

    /// Copies a character string to the terminal.
    let mvwaddchnstr win y x chstr n =
        Imported.mvwaddchnstr(win, y, x, chstr, n) |> verify "mvwaddchnstr"

    /// Copies a string to the terminal.
    let addstr str =
        Imported.addstr(str) |> verify "addstr"

    /// Copies a string to the terminal.
    let addnstr str n =
        Imported.addnstr(str,n) |> verify "addnstr"

    /// Copies a string to the terminal.
    let waddstr win str =
        Imported.waddstr(win,str) |> verify "waddstr"

    /// Copies a string to the terminal.
    let waddnstr win str n =
        Imported.waddnstr(win,str,n) |> verify "waddnstr"

    /// Copies a string to the terminal.
    let mvaddstr y x str =
        Imported.mvaddstr(y,x,str) |> verify "mvaddstr"

    /// Copies a string to the terminal.
    let mvaddnstr y x str n =
        Imported.mvaddnstr(y,x,str,n) |> verify "mvaddnstr"

    /// Copies a string to the terminal.
    let mvwaddstr win y x str =
        Imported.mvwaddstr(win,y,x,str) |> verify "mvwaddstr"

    /// Copies a string to the terminal.
    let mvwaddnstr win y x str n =
        Imported.mvwaddnstr(win,y,x,str,n) |> verify "mvwaddnstr"

    /// Save the current terminal modes as the "program".
    let def_prog_mode () =
        Imported.def_prog_mode() |> verify "def_prog_mode"

    /// Save the current terminal modes as the "shell".
    let def_shell_mode () =
        Imported.def_shell_mode() |> verify "def_shell_mode"

    /// Restore the terminal to "program" state.
    let reset_prog_mode () =
        Imported.reset_prog_mode() |> verify "reset_prog_mode"

    /// Restore the terminal to "shell" state
    let reset_shell_mode () =
        Imported.reset_shell_mode() |> verify "reset_shell_mode"

    /// Restore the state of the terminal modes from a buffer.
    let resetty () =
        Imported.resetty() |> verify "resetty"

    /// Save the state of the terminal modes to a buffer.
    let savetty () =
        Imported.savetty() |> verify "savetty"

    /// Get the current coordinates of the virtual screen cursor.
    let getsyx () =
        let mutable y,x = 0,0
        Imported.getsyx(&y, &x)
        y,x

    /// Set the coordinates of the virtual screen cursor.
    let setsyx y x =
        Imported.setsyx(y, x)

    /// Removes a line from the stdscr.
    let ripoffline line init =
        Imported.ripoffline(line, init) |> verify "ripoffline"

    /// Sets the cursor state to invisible, normal or very visible.
    let curs_set visibility =
        Imported.curs_set(visibility) |> verify "curs_set"

    /// Sleep for ms milliseconds.
    let napms ms =
        Imported.napms(ms) |> verify "napms"

    let move y x =
        Imported.move(y, x) |> verify "move"

    let wmove win y x =
        Imported.wmove(win, y, x) |> verify "wmove"

    let attroff attrs =
        Imported.attroff(attrs) |> verify "attroff"

    let wattroff win attrs =
        Imported.wattroff(win, attrs) |> verify "wattroff"

    let attron attrs =
        Imported.attron(attrs) |> verify "attron"

    let wattron win attrs =
        Imported.wattron(win, attrs) |> verify "wattron"

    let attrset attrs =
        Imported.attrset(attrs) |> verify "attrset"

    let wattrset win attrs =
        Imported.wattrset(win, attrs) |> verify "wattrset"

    let color_set color_pair_number =
        Imported.color_set(color_pair_number, IntPtr.Zero) |> verify "color_set"

    let wcolor_set win color_pair_number =
        Imported.wcolor_set(win, color_pair_number, IntPtr.Zero) |> verify "wcolor_set"

    let standend () =
        Imported.standend() |> verify "standend"

    let wstandend win =
        Imported.wstandend(win) |> verify "wstandend"

    let standout () =
        Imported.standout() |> verify "standout"

    let wstandout win =
        Imported.wstandout(win) |> verify "wstandout"

    let attr_get () =
        // TODO: clean up attr_get
        let mutable attrs,pair = 0u,0s
        let result = Imported.attr_get(&attrs, &pair, IntPtr.Zero) 
        match verify "attr_get" result with 
        | Success _ -> Result.result (attrs,pair)
        | Failure e -> Failure e

    let wattr_get win =
        // TODO: clean up wattr_get
        let mutable attrs,pair = 0u,0s
        let result = Imported.wattr_get(win, &attrs, &pair, IntPtr.Zero) 
        match verify "wattr_get" result with 
        | Success _ -> Result.result (attrs,pair)
        | Failure e -> Failure e

    let attr_off attrs =
        Imported.attr_off(attrs, IntPtr.Zero) |> verify "attr_off"

    let wattr_off win attrs opts =
        Imported.wattr_off(win, attrs, IntPtr.Zero) |> verify "wattr_off"

    let attr_on attrs opts =
        Imported.attr_on(attrs, IntPtr.Zero) |> verify "attr_on"

    let wattr_on win attrs opts =
        Imported.wattr_on(win, attrs, IntPtr.Zero) |> verify "wattr_on"

    let attr_set attrs pair opts =
        Imported.attr_set(attrs, pair, IntPtr.Zero) |> verify "attr_set"

    let wattr_set win attrs pair opts =
        Imported.wattr_set(win, attrs, pair, IntPtr.Zero) |> verify "wattr_set"

    let chgat n attr color opts =
        Imported.chgat(n, attr, color, IntPtr.Zero) |> verify "chgat"

    let wchgat win n attr color opts =
        Imported.wchgat(win, n, attr, color, IntPtr.Zero) |> verify "wchgat"

    let mvchgat y x n attr color opts =
        Imported.mvchgat(y, x, n, attr, color, IntPtr.Zero) |> verify "mvchgat"

    let mvwchgat win y x n attr color opts =
        Imported.mvwchgat(win, y, x, n, attr, color, IntPtr.Zero) |> verify "mvwchgat"

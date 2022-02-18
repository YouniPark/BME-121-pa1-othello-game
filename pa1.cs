#nullable enable
using System;
using static System.Console;

namespace Bme121
{
    record Player( string Colour, string Symbol, string Name );
    
    // The `record` is a kind of automatic class in the sense that the compiler generates
    // the fields and constructor and some other things just from this one line.
    // There's a rationale for the capital letters on the variable names (later).
    // For now you can think of it as roughly equivalent to a nonstatic `class` with three
    // public fields and a three-parameter constructor which initializes the fields.
    // It would be like the following. The 'readonly' means you can't change the field value
    // after it is set by the constructor method.
    
    //class Player
    //{
        //public readonly string Colour;
        //public readonly string Symbol;
        //public readonly string Name;
        
        //public Player( string Colour, string Symbol, string Name )
        //{
            //this.Colour = Colour;
            //this.Symbol = Symbol;
            //this.Name = Name;
        //}
    //}
    
    static partial class Program
    {
        // Display common text for the top of the screen.
        
        static void Welcome( )
        {
			WriteLine( "Welcome to Othello. Let's play the game." );
        }
        
        // Collect a player name or default to form the player record.
        
        static Player NewPlayer( string colour, string symbol, string defaultName )
        {
			Write( "Type the {0} disc ({1}) player name [or <Enter> for name: '{2}'] : ", colour, symbol, defaultName );
			string name = ReadLine()!;
			if ( name.Length == 0 ) name = defaultName;
            return new Player( colour, symbol , name );
        }
        
        // Determine which player goes first or default.
        
        static int GetFirstTurn( Player[ ] players, int defaultFirst )
        {
			while(true)
			{
				Write( "Choose who will play first [or <Enter> for {0}/{1}/{2}]: ",
						players [ defaultFirst ].Colour,
						players [ defaultFirst ].Symbol, 
						players [ defaultFirst ].Name ) ;
				string response = ReadLine ()!;
				if (response.Length == 0 ) return defaultFirst; // makes the first player goes first
				for ( int i = 0; i < players.Length ; i ++)
				{ 
					if ( players[i].Colour == response ) return i; // when user typed colour
					if ( players[i].Symbol == response ) return i; // when user typed symbol
					if ( players[i].Name == response ) return i; // when user typed name
				}
				WriteLine( "Invalid response, please try again." );
			}
        }
        
        // Get a board size (between 4 and 26 and even) or default, for one direction.
        
        static int GetBoardSize( string direction, int defaultSize )
        {
			while(true)
			{
				Write( $"Enter board {direction} (4 to 26, even) [or <ENTER> for default size: {defaultSize} ]: ");
				string response = ReadLine()!;
				if( response.Length == 0 ) return defaultSize;
				int size = int.Parse(response);
				
				if( size >= 4 && size <= 26 && size % 2 == 0 ) return size;
				WriteLine( "Invalid response, please try again." );
			}
        }
        
        // Get a move from a player.
        
        static string GetMove( Player player )
        {
            WriteLine( $"\nThis turn is {player.Colour} disc ({player.Symbol}) player: {player.Name}.");
            WriteLine( "pick a cell by its row & columns names (i.e. 'ad' ) to play there." );
            Write( "OR enter 'skip' to skip the turn or 'quit' to end the game: " );
            return ReadLine ()!;
        }
        
        // Try to make a move. Return true if it worked.
        
        static bool TryMove( string[ , ] board, Player player, string move )
        {
			// If move is 'skip' return true ( no action needed).
			// If move length is not 2, return false
			// Get first and last substrings of length 1 ( move.Substring(0,1))
			// Using IindexAtLetter(), if index of either is -1, return false
			// Otherwise save the row/col as the move (in local variables)
			// If row or column too big for board, return false
			// If board occupied at that spot, return false
			// Temporary: put the player symbol at that location
			// Actual: Call TryDirection eight times and keep track of whether any return true
			// If so return true
			// OtherWise return false
			
			const string empty = " ";
			if( move == "skip" ) return true; 
			
			if( move.Length != 2 )
			{
				WriteLine( "\n You should enter two characters for move (i.e 'fc' )," );
				WriteLine( "first letter is for the row and the sexond letter is for the column. "); 
				return false; // when it's not worked
			}
			
			int row = IndexAtLetter( move.Substring(0,1) );
			if( row < 0 || row >= board.GetLength(0) ) // int converts to letter by IndexAtLetter, so we can use integer
			{
				WriteLine( "\n The first character must be a row in the game board." );
				return false;
			}
			
			int col = IndexAtLetter( move.Substring(1,1) );
			if( col < 0 || col >= board.GetLength(1) )  
			{
				WriteLine( "\n The second character must be a column in the game board." );
				return false;
			}
			
			if( board[ row, col ] != empty )
			{
				WriteLine( "\n the cell you chose is already occupied.");
				return false;
			}
						
            bool [] valid = new bool [8] ;
            
            valid[0] = TryDirection( board, player, row, -1, col,  0 ) ; // N, when the direction is not possible, TryDirection method get valid=false
            valid[1] = TryDirection( board, player, row, -1, col,  1 ) ; // NE
            valid[2] = TryDirection( board, player, row,  0, col,  1 ) ; // E
            valid[3] = TryDirection( board, player, row,  1, col,  1 ) ; // SE
            valid[4] = TryDirection( board, player, row,  1, col,  0 ) ; // S
            valid[5] = TryDirection( board, player, row,  1, col, -1 ) ; // SW
            valid[6] = TryDirection( board, player, row,  0, col, -1 ) ; // W
            valid[7] = TryDirection( board, player, row, -1, col, -1 ) ; // NW
			
			for( int i = 0; i < valid.Length; i ++)
			{
				if( valid[i] == true)
				{ 
					return true;
				}
			}
			return false;
        }
        
        // Do the flips along a direction specified by the row and column delta for one step.
        
        static bool TryDirection( string[ , ] board, Player player,
            int moveRow, int deltaRow, int moveCol, int deltaCol )
        {			
			const string empty = " ";
			
			// Check whether the neighbouring cell on the specified direction 
			int nextRow = moveRow + deltaRow;
			int nextCol = moveCol + deltaCol;
			
			if( nextRow < 0 || nextRow >= board.GetLength(0) ) return false;
			if( nextCol < 0 || nextCol >= board.GetLength(1) ) return false;
						
			// Pass? If the first neighbouring cell is player's colour return false
			if( board[ nextRow, nextCol ] == player.Symbol ) return false;
			
			// Pass? Line continues to the player's colour?
			// Count the discs which will be flipped
			
			int count = 1;
			bool validMove = true;			
			while( validMove )
			{
				// Is there the next cell on the board?
				nextRow = nextRow + deltaRow;
				nextCol = nextCol + deltaCol;
				
				if( nextRow < 0 || nextRow >= board.GetLength(0) ) validMove = false; 
				else if( nextCol < 0 || nextCol >= board.GetLength(1) ) validMove = false; 
				
				// Pass? Is the next cell empty?
				
				else if( board[ nextRow, nextCol ] == empty ) validMove = false;
				
				// Pass? Does the next cell hold the player's colour?
				
				else if( board[ nextRow, nextCol ] != player.Symbol ) count ++; 
				else
				{		
					board[ moveRow, moveCol ] = player.Symbol;
					
					for( int c = 0; c < count ; c ++) 
					{
						moveRow += deltaRow;
						moveCol += deltaCol; 
						
						board[ moveRow, moveCol ] = player.Symbol;
					}
					
					return true;
				}
			}
            return false;
        }
        
        // Count the discs to find the score for a player.
        
        static int GetScore( string[ , ] board, Player player )
        {
			int score = 0; 

			for( int r = 0; r < board.GetLength(0); r ++)
			{
				for( int c = 0; c < board.GetLength(1); c ++)
				{
					if( board[ r,c ] == player.Symbol )
					{
						score ++; 
					}
				}
			}
			return score;
        }
        
        // Display a line of scores for all players.
        
        static void DisplayScores( string[ , ] board, Player[ ] players )
        {
			int score1 = GetScore( board, players[0] );
			int score2 = GetScore( board, players[1] );
			
			WriteLine( $"{players[0].Name} got score: {score1} \n{players[1].Name} got score: {score2}" );
        }
        
        // Display winner(s) and categorize their win over the defeated player(s).
        
        static void DisplayWinners( string[ , ] board, Player[ ] players )
        {
			if ( GetScore( board, players[0] ) > GetScore( board, players[1] ) ) 
				{
					WriteLine( $"\nCongratulation! {players[0].Name} won. \n");
					WriteLine( $"1st: {players[0].Name} | Score : {GetScore(board, players[0])} \n2st: {players[1].Name} | Score : {GetScore(board, players[1])} " );
				}	
			else if ( GetScore( board, players[0] ) == GetScore( board, players[1] ) ) 
				WriteLine( $"\nThis game was tie." );
			else // if ( GetScore( board, players[0] ) < GetScore( board, players[1] ) ) 
				{
					WriteLine( $"\nCongratulation! {players[1].Name} won. \n" );
					WriteLine( $"1st: {players[1].Name} | Score : {GetScore(board, players[1])} \n2st: {players[0].Name} | Score : {GetScore(board, players[0])} " );
				}
        }
        
        static void Main( )
        {
            // Set up the players and game.
            // Note: I used an array of 'Player' objects to hold information about the players.
            // This allowed me to just pass one 'Player' object to methods needing to use
            // the player name, colour, or symbol in 'WriteLine' messages or board operation.
            // The array aspect allowed me to use the index to keep track or whose turn it is.
            // It is also possible to use separate variables or separate arrays instead
            // of an array of objects. It is possible to put the player information in
            // global variables (static field variables of the 'Program' class) so they
            // can be accessed by any of the methods without passing them directly as arguments.
            
            Welcome( );
            
            Player[ ] players = new Player[ ] 
            {
                NewPlayer( colour: "black", symbol: "X", defaultName: "Black" ),
                NewPlayer( colour: "white", symbol: "O", defaultName: "White" ),
            };
            
            int turn = GetFirstTurn( players, defaultFirst: 0 );
           
            int rows = GetBoardSize( direction: "rows",    defaultSize: 8 );
            int cols = GetBoardSize( direction: "columns", defaultSize: 8 );
            
            string[ , ] game = NewBoard( rows, cols );
            
            // Play the game.
            
            bool gameOver = false;
            while( ! gameOver )
            {
                Welcome( );
                DisplayBoard( game ); 
                DisplayScores( game, players );
                
                string move = GetMove( players[ turn ] );
                if( move == "quit" ) gameOver = true;
                else
                {
                    bool madeMove = TryMove( game, players[ turn ], move );
                    if( madeMove ) turn = ( turn + 1 ) % players.Length;
                    else 
                    {
                        Write( " Your choice didn't work!" );
                        Write( " Press <Enter> to try again." );
                        ReadLine( ); 
                    }
                }
            }
            
            // Show fhe final results.
            
            DisplayWinners( game, players );
            WriteLine( );
        }
    }
}

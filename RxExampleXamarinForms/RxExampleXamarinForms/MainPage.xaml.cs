using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RxExampleXamarinForms
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

#if DEBUG1
            InitRxCorrect();
#elif DEBUG2
            InitRxNoConcat();
#endif
        }
        
        void InitRxCorrect()
        {
            var sub = Observable.FromEventPattern<TextChangedEventArgs>(searchEntry, nameof(Entry.TextChanged))
                .Where(evt => evt.EventArgs.NewTextValue?.Length >= 3) // text length at least 3 characters
                .Throttle(TimeSpan.FromMilliseconds(500)) // passing event max 1 time for 500 ms
                .Select(evt => GetMovies(evt.EventArgs.NewTextValue)) // get event argument and pass to async function
                .Concat() // keep async function results in order
                .ObserveOn(System.Threading.SynchronizationContext.Current) // observe on current thread (UI) to update ui in subscription
                .Subscribe(results => searchResults.Text = string.Join(Environment.NewLine, results)); // show results in a label
        }

        void InitRxNoConcat()
        {
            Observable.FromEventPattern<TextChangedEventArgs>(searchEntry, nameof(Entry.TextChanged))
                .Select(evt => evt.EventArgs.NewTextValue)
                .SelectMany(term =>
                {
                    Debug.WriteLine($"[searchObservable.SelectMany] Text: {term}");
                    return Observable.FromAsync(() => GetMovies(term));
                })
                // For Concat we need result from Select, without Concat we need SelectMany
                // You can comment SelectMany above and uncomment Select and Concat below to see that responses will now delivered in correct order
                /*.Select(term =>
                {
                    Debug.WriteLine($"[searchObservable.Select]  Text: {term}");
                    return Observable.FromAsync(() => GetMovies(term));
                })
                .Concat()*/
                .Subscribe(results =>
                {
                    Debug.WriteLine($"[searchObservable.Subscribe] Results: {results.FirstOrDefault()}");
                });
        }
        
        string[] _movieDb = new[] { "The Fifth Element", "The Martian", "Blade Runner", "Spider-Man", "Baby Driver", "Divergent", "X-Men: Days of Future Past", "Alien: Covenant", "Beauty and the Beast", "The Dark Knight", "The Hobbit: The Battle of the Five Armies", "Despicable Me 3", "How to Train Your Dragon 2", "Cinderella", "Fight Club", "Batman Begins", "Inception", "Birdman", "Despicable Me 2", "Deadpool", "The Avengers", "The Hobbit: The Desolation of Smaug", "Schindler's List", "Ice Age: The Meltdown", "Alice in Wonderland", "Cars", "Pirates of the Caribbean: At World's End", "Titanic", "American Sniper", "The Empire Strikes Back", "Black Swan", "Prisoners", "Pirates of the Caribbean: On Stranger Tides", "Baywatch", "Fifty Shades of Grey", "The Shawshank Redemption", "The Shining", "Iron Man", "The Lord of the Rings: The Fellowship of the Ring", "Jurassic World", "Mission: Impossible - Rogue Nation", "John Wick", "Harry Potter and the Order of the Phoenix", "Split", "Spider-Man: Homecoming", "Batman v Superman: Dawn of Justice", "2001: A Space Odyssey", "A Clockwork Orange", "X-Men: Apocalypse", "La La Land", "Rogue One: A Star Wars Story", "Django Unchained", "Ice Age", "Kong: Skull Island", "mother!", "Star Wars: The Force Awakens", "Man of Steel", "Fury", "Minions", "Skyfall", "Night at the Museum: Secret of the Tomb", "World War Z", "It Follows", "I, Robot", "Dawn of the Planet of the Apes", "Chappie", "The Layover", "Harry Potter and the Deathly Hallows: Part 2", "Kingsman: The Golden Circle", "Terminator 2: Judgment Day", "Harry Potter and the Prisoner of Azkaban", "Star Trek Into Darkness", "Taken 3", "Big Hero 6", "American Beauty", "Exodus: Gods and Kings", "We're the Millers", "Arrival", "Pan's Labyrinth", "Frozen", "Alien", "Pirates of the Caribbean: Dead Men Tell No Tales", "One Flew Over the Cuckoo's Nest", "Edge of Tomorrow", "The Godfather: Part II", "Gravity", "The Green Mile", "Leon: The Professional", "Miss Peregrine's Home for Peculiar Children", "Avatar", "Captain America: Civil War", "John Wick: Chapter 2", "Thor", "The Hobbit: An Unexpected Journey", "Kill Bill: Vol. 1", "Harry Potter and the Half-Blood Prince", "Beauty and the Beast", "Pirates of the Caribbean: Dead Man's Chest", "Iron Man 3", "The Maze Runner", "Harry Potter and the Philosopher's Stone", "Guardians of the Galaxy Vol. 2", "Suicide Squad", "Whiplash", "Hacksaw Ridge", "The Mummy", "Annabelle: Creation", "Finding Nemo", "Men in Black II", "The Revenant", "The Jungle Book", "Se7en", "The Imitation Game", "Gone Girl", "Twilight", "Mad Max: Fury Road", "The Amazing Spider-Man", "The Hitman's Bodyguard", "Logan", "Kingsman: The Secret Service", "Monsters University", "The Expendables", "Wonder Woman", "Ex Machina", "Psycho", "Aladdin", "Pirates of the Caribbean: The Curse of the Black Pearl", "12 Years a Slave", "Quantum of Solace", "Inside Out", "Taken", "Men in Black", "The Godfather", "The Wolf of Wall Street", "Tomorrowland", "The Fate of the Furious", "The Hangover", "The Twilight Saga: Breaking Dawn - Part 1", "Pulp Fiction", "The Dark Knight Rises", "The Incredibles", "The Lord of the Rings: The Return of the King", "Indiana Jones and the Last Crusade", "The Hunger Games: Mockingjay - Part 2", "Brave", "Pixels", "Guardians of the Galaxy", "The Hunger Games: Mockingjay - Part 1", "Despicable Me", "San Andreas", "Spectre", "Jupiter Ascending", "Get Out", "Spirited Away", "The Bourne Identity", "Furious 7", "The Amazing Spider-Man 2", "Interstellar", "Star Wars", "Lucy", "Thor: The Dark World", "The Lord of the Rings: The Two Towers", "Forrest Gump", "Sherlock Holmes: A Game of Shadows", "Maleficent", "Monsters, Inc.", "Ghost in the Shell", "Nightcrawler", "Doctor Strange", "Up", "Shutter Island", "Casino Royale", "It", "Mission: Impossible - Ghost Protocol", "Insurgent", "Finding Dory", "Life Is Beautiful", "Terminator Genisys", "V for Vendetta", "Zootopia", "Avengers: Age of Ultron", "The Matrix", "Passengers", "The Twilight Saga: Breaking Dawn - Part 2", "Harry Potter and the Goblet of Fire", "Rise of the Planet of the Apes", "Teenage Mutant Ninja Turtles", "Fantastic Beasts and Where to Find Them", "The Devil Wears Prada", "Dunkirk", "Maze Runner: The Scorch Trials", "Harry Potter and the Chamber of Secrets", "Gladiator", "Spider-Man 3", "Ant-Man", "The Lion King", "Kung Fu Panda", "Bruce Almighty", "Transformers: Age of Extinction", "The Equalizer" };
        Random _rand = new Random();
        public async Task<IEnumerable<string>> GetMovies(string searchTerm)
        {
            await Task.Delay(100 * _rand.Next(1, 6));
#if DEBUG1
            return _movieDb.Where(s => s.ToLower().Contains(searchTerm.ToLower().Trim()));
#elif DEBUG2
            return new List<string> { searchTerm };
#endif
        }
    }
}

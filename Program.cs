using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using static System.Console;

namespace cookapp
{
    class Program
    {
        /// <summary>
        /// TryParseFloat() permet de convertir un string en float. Tant que l'utilisateur rentre des valeurs non convertissables,
        /// il doit continuer à rentrer des valeurs
        /// </summary>
        /// <returns>string converti en float</returns>
        public static float TryParseFloat()
        {
            float result;
            string val = ReadLine();
            while (float.TryParse(val, out result) == false)
            {
                val = ReadLine();
            }
            return result;
        }

        /// <summary>
        /// ActionsClient(MySqlConnection connexion) crée un compte client (appel de la méthode creerclient() à l'utilisateur si celui déclare ne pas en avoir.
        /// Ensuite le client peut se connecter et a le choix entre commander,afficher ses commandes ou demander à devenir cdr.
        /// Pour utiliser les fonctions allouées au cdr, il devra se déconnecter. Il est ici connecté comme client.
        /// Les différents choix sont représentées par un switch avec des ConsoleKey faisant ensuite appel aux méthodes correspondantes dans la classe Client.
        /// Si la touche tapée est différente de celles proposées, il ne se passe rien (d'ou les while englobant les switch)
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void ActionsClient(MySqlConnection connexion)
        {
            WriteLine("Possédez vous un compte client ? Tapez [O] si Oui, tapez [N] sinon");
            bool erreur = true;
            while (erreur == true)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.N:
                        Clear();
                        erreur = false;
                        Client.CreerClient(connexion);
                        Clear();
                        break;
                    case ConsoleKey.O:
                        Clear();
                        erreur = false;
                        break;
                    default:
                        break;
                }
            }
            Client client = Client.Login(connexion, "codeClient", "client");
            Clear();
            WriteLine("Choisissez une action: Commander: [C]" + "\n" + "Affichage de mes commandes: [A]" + "\n" + "Je souhaite devenir créateur de recettes: [R]");
            bool bonneentrée = false;
            while (bonneentrée == false)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.C:
                        Clear();
                        bonneentrée = true;
                        client.Commander(connexion);
                        ReadKey();
                        break;
                    case ConsoleKey.A:
                        Clear();
                        bonneentrée = true;
                        client.ListeCommandes(connexion);
                        ReadKey();
                        break;
                    case ConsoleKey.R:
                        Clear();
                        bonneentrée = true;
                        client.ClientToCdr(connexion);
                        ReadKey();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// ActionsCdr(MySqlConnection connexion) oblige tout d'abord le cdr à se connecter avant de pouvoir agir.
        /// Ensuite le cdr a le choix entre créer,afficher son solde de cooks ou afficher la liste de ses recettes.
        /// Les différents choix sont représentées par un switch avec des ConsoleKey faisant ensuite appel aux méthodes correspondantes dans la classe Cdr.
        /// Si la touche tapée est différente de celles proposées, il ne se passe rien (d'ou le while englobant le switch)
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void ActionsCdr(MySqlConnection connexion)
        {
            CreateurDeRecettes cdr = CreateurDeRecettes.Login(connexion, "codeCdr", "cdr");
            Clear();
            WriteLine("Choisissez une action: créer une recette: [R]" + "\n" + "Affichage de mon solde cook: [C]" + "\n" + "Affichage de mes recettes [A] commandées");
            bool bonneentrée = false;
            while (bonneentrée == false)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.C:
                        Clear();
                        bonneentrée = true;
                        cdr.SoldeCook(connexion);
                        ReadKey();
                        break;
                    case ConsoleKey.A:
                        Clear();
                        bonneentrée = true;
                        cdr.ListeCommandes(connexion);
                        ReadKey();
                        break;
                    case ConsoleKey.R:
                        Clear();
                        bonneentrée = true;
                        cdr.CreationRecette(connexion);
                        ReadKey();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// ActionsAdmin(MySqlConnection connexion) demande tout d'abord à rentrer l'unique code admin pour accéder aux fonctions de la classe Admin.
        /// Si un mauvais code est entré (comparaison avec la propriété statique d'Admin), l'utilisateur est ejecté directement.
        /// Sinon, il a le choix entre afficher le cdr le plus commandé sur les 7 derniers jours, les 5 recettes les plus commandées depuis l'ouverture du site,
        /// ou afficher le cdr le plus commandé depuis l'ouverture du site.
        /// Il peut également supprimer un créateur de recettes ou une recette.
        /// Les fonctions utilisées sont celles de la classe Admin (statiques)
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void ActionsAdmin(MySqlConnection connexion)
        {
            WriteLine("Veuillez entrer le code admin");
            string code = ReadLine();
            if(code != Admin.Mdp)
            {
                WriteLine("Accès refusé.");
                ReadKey();
            }
            else
            {
                WriteLine("Choisissez une action: Afficher le cdr de la semaine: [D]" + "\n" + "Affichage le top 5 des recettes: [5]" + "\n" + "Afficher le cdr d'or [O]" + "\n" + "Supprimer une recette [R]" + "\n" + "Supprimer un créateur de recettes [C]");
                bool bonneentrée = false;
                while (bonneentrée == false)
                {
                    switch (ReadKey(true).Key)
                    {
                        case ConsoleKey.D:
                            Clear();
                            bonneentrée = true;
                            Admin.TopSemaine(connexion);
                            ReadKey();
                            break;

                        case ConsoleKey.D5:
                            Clear();
                            bonneentrée = true;
                            Admin.TopCinqRecettes(connexion);
                            ReadKey();
                            break;

                        case ConsoleKey.O:
                            Clear();
                            bonneentrée = true;
                            Admin.CdrOr(connexion);
                            ReadKey();
                            break;

                        case ConsoleKey.R:
                            Clear();
                            bonneentrée = true;
                            Admin.SupprimerRecette(connexion);
                            break;

                        case ConsoleKey.C:
                            Clear();
                            bonneentrée = true;
                            Admin.SupprimerCdr(connexion);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///  ActionsCooker(MySqlConnection connexion) permet à un cuisinier de se connecter, et lui donne le choix de cuisiner.
        ///  Si les commandes de la journée ont toutes été cuisinées/livrées, les commandes pour le lendemain sont faites.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void ActionsCooker(MySqlConnection connexion)
        {
            Cook cook = Cook.Login(connexion);
            Clear();
            WriteLine("Voulez vous cuisiner maintenant ? Oui: [O]" + "\n" + "Non: [N]");
            bool bonneentrée = false;
            while (bonneentrée == false)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.O:
                        Clear();
                        bonneentrée = true;
                        cook.Cuisine(connexion);
                        ReadKey();
                        break;
                    case ConsoleKey.N:
                        Clear();
                        bonneentrée = true;
                        break;
                    default:
                        break;
                    case ConsoleKey.X:
                        Clear();
                        Cook.ListeXML(connexion);
                        ReadKey();
                        break;
                }
            }

            MySqlCommand command = connexion.CreateCommand();
            command.CommandText = "SELECT count(*) FROM cook.commande WHERE day(dateLivraison) = day(current_timestamp()) and month(dateLivraison) = month(current_timestamp()) and livree = 'non';";
            MySqlDataReader reader = command.ExecuteReader();
            int compteur = 0;
            while (reader.Read())
            {
                compteur = reader.GetInt32(0);
            }
            if(compteur == 0)
            {
               Cook.ListeXML(connexion);
            }
        }

        /// <summary>
        /// MenuClient(MySqlConnection connexion) donne le choix à un client de se connecter comme client ou comme cdr.
        /// S'il décide de se connecter comme client, la méthode ActionsClient() précédente sera appelée, et ActionsCdr() sinon,
        /// donnant respectivement accès aux fonctions du client ou du cdr
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void MenuClient(MySqlConnection connexion)
        {
            WriteLine("Je souhaite me connecter comme: " + "\n" + "client [C]" + "\n" + "créateur de recettes [R]");
            bool bonneentrée = false;
            while (bonneentrée == false)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.R:
                        Clear(); 
                        bonneentrée = true;
                        ActionsCdr(connexion);
                        break;
                    case ConsoleKey.C:
                        Clear();
                        bonneentrée = true;
                        ActionsClient(connexion);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// MenuPro(MySqlConnection connexion) permet à un professionnel de se connecter comme cuisinier ou fournisseur.
        /// </summary>
        public static void MenuPro(MySqlConnection connexion)
        {
            WriteLine("Je suis " + "\n" + "cuisinier: [C]" + "\n" + "fournisseur [F]");
            bool bonneentrée = false;
            while (bonneentrée == false)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.C:
                        Clear();
                        bonneentrée = true;
                        ActionsCooker(connexion);
                        break;
                    case ConsoleKey.F:
                        Clear();
                        bonneentrée = true;
                        break;
                    default:
                        break;
                }
            }

        }

        /// <summary>
        /// MenuDemo(MySqlConnection connexion) correspond au menu permettant d'accéder aux fonctions évaluateurs.
        /// Vous pouvez choisir, en tapant sur les touches indiquées, d'afficher le nombre de clients de l'application, le nombre de recettes,
        /// la liste des cdr, la liste des produits dont le stock actuel est inférieur au double du stock minimal, ainsi que les recettes contenant ces produits.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void MenuDemo(MySqlConnection connexion)
        {
            WriteLine("Bienvenue sur le menu Demo:");
            WriteLine("Choisissez une action: Affichage du nombre de clients: [C]" + "\n" + "Affichage du nombre de recettes: [N]" + "\n" + "Affichage des créateurs de recette: [R]" + "\n" + "Liste des produits en basse quantité: [P]" + "\n" + "Affichage des recettes contenant les produits en basse quantité: [B]");
            bool bonneentrée = false;
            while (bonneentrée == false)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.C:
                        Clear();
                        bonneentrée = true;
                        WriteLine("Ma Petite Cuisine compte actuellement " + BaseDeDonnées.CompterTuplesTable(connexion, "cook.client;") +" clients");
                        ReadKey();
                        break;
                    case ConsoleKey.N:
                        Clear();
                        bonneentrée = true;
                        WriteLine("Ma Petite Cuisine compte actuellement " + BaseDeDonnées.CompterTuplesTable(connexion, "cook.recette;") + " recettes");
                        ReadKey();
                        break;
                    case ConsoleKey.R:
                        Clear();
                        bonneentrée = true;
                        BaseDeDonnées.AfficherCdrs(connexion);
                        ReadKey();
                        break;
                    case ConsoleKey.P:
                        Clear();
                        bonneentrée = true;
                        BaseDeDonnées.BasseQuantite(connexion, "2*");
                        ReadKey();
                        break;
                    case ConsoleKey.B:
                        Clear();
                        bonneentrée = true;
                        BaseDeDonnées.RecettesBasseQuantite(connexion);
                        ReadKey();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// La méthode main permet à l'utilisateur selon qu'il soit client (client/cdr), professionnel, administrateur ou professeur (demo)
        /// d'avancer dans l'application jusqu'aux commandes de connection puis les fonctionnalités correspondant à leur statut.
        /// Les méthodes Menu ci-dessus sont appelées selon la lettre tapée par l'utilisateur.
        /// Une fois son action terminée, l'utilisateur peut continuer sur l'appli ou se déconnecter en tapant N.
        /// </summary>
        static void Main()
        {
            BaseDeDonnées database = new BaseDeDonnées();
            MySqlConnection connexion = database.connexion();

            WriteLine("Bienvenue sur Ma Petite Cuisine:" + "\n" + "Je suis client: [C]" + "\n" + "Je suis professionnel: [P]" + "\n" + "Je suis administrateur [A]" + "\n" + "Je souhaite accéder au mode démo: [D]");
            bool bonneentrée = false;
            bool end = false;
            while (bonneentrée == false)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.C:
                        Clear(); 
                        bonneentrée = true;
                        while(end == false)
                        {
                            MenuClient(connexion);
                            Clear();
                            WriteLine("Pour vous déconnecter tapez [N] sinon  pour continuer,appuyez sur n'importe quelle autre touche");
                            if(ReadKey(true).Key == ConsoleKey.N)
                            {
                                end = true;
                            }
                            Clear();
                        }                       
                        break;

                    case ConsoleKey.P:
                        Clear(); 
                        bonneentrée = true;
                        while (end == false)
                        {
                            MenuPro(connexion);
                            Clear();
                            WriteLine("Pour vous déconnecter tapez [N] sinon  pour continuer,appuyez sur n'importe quelle autre touche");
                            if (ReadKey(true).Key == ConsoleKey.N)
                            {
                                end = true;
                            }
                            Clear();
                        }
                        break;

                    case ConsoleKey.D:
                        Clear();
                        bonneentrée = true;
                        while (end == false)
                        {
                            MenuDemo(connexion);
                            Clear();
                            WriteLine("Pour vous déconnecter tapez [N] sinon  pour continuer,appuyez sur n'importe quelle autre touche");
                            if (ReadKey(true).Key == ConsoleKey.N)
                            {
                                end = true;
                            }
                            Clear();
                        }

                        break;
                    case ConsoleKey.A:
                        Clear();
                        bonneentrée = true;
                        while (end == false)
                        {
                            ActionsAdmin(connexion);
                            Clear();
                            WriteLine("Pour vous déconnecter tapez [N] sinon  pour continuer,appuyez sur n'importe quelle autre touche");
                            if (ReadKey(true).Key == ConsoleKey.N)
                            {
                                end = true;
                            }
                            Clear();
                        }

                        break;
                    default:
                        break;
                }
            }
            WriteLine("Merci d'avoir utilisé notre application ! A bientot !");
            database.Deconnexion(connexion);

            ReadKey();

        }
    }
}

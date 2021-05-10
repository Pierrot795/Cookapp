using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using MySql.Data.MySqlClient;

namespace cookapp
{
    static class Admin
    {
        /// <summary>
        /// Mdp correspond au code pour accéder à l'interface administrateur
        /// </summary>
        public static string Mdp { get { return "2736jpLkA"; } }

        #region méthodes
        /// <summary>
        /// SupprimerCdr(MySqlConnection connexion) affiche tout d'abord la liste des cdr pour permettre à l'admin de choisir qui supprimer.
        /// S'il tape un code cdr qui n'existe pas, il doit en retaper un.
        /// Il a ensuite le choix de le laisser client ou non, avec un switch à deux cas.
        /// Dans le premier cas, on va supprimer le cdr de la table cook.cdr, ce qui va supprimer ses recettes.
        /// Dans le second cas, on supprime le client, donc le cdr est également supprimé.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void SupprimerCdr(MySqlConnection connexion) 
        {
            BaseDeDonnées.AfficherCdrs(connexion);

            WriteLine("Tapez le code du cdr à supprimer");
            string cdr = ReadLine();
            while (BaseDeDonnées.VerifExistenceInstance(connexion, cdr, "codeCdr", "cdr") == false)
            {
                WriteLine("Veuillez entrer le code d'un créateur de recettes existant"); 
            }

            MySqlCommand suppressionCommand = connexion.CreateCommand();
            WriteLine("Souhaitez vous que cette personne reste cliente ? " + "\n" + "Oui   : [O]" + "\n" + "Non  : [N]");
            suppressionCommand.Parameters.Add(new MySqlParameter("@code", MySqlDbType.VarChar, value: cdr, size: 4, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));

            bool bonneentrée = false;
            while (bonneentrée == false)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.N:
                        Clear(); //
                        bonneentrée = true;
                        suppressionCommand.CommandText = "DELETE FROM cook.cdr c  where c.codeCdr =@code;";
                        BaseDeDonnées.NonQuery(suppressionCommand);
                        break;
                    case ConsoleKey.O:
                        Clear(); //
                        bonneentrée = true;
                        suppressionCommand.CommandText = "DELETE FROM cook.client c  where c.codeClient =@code;";
                        BaseDeDonnées.NonQuery(suppressionCommand);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// SupprimerRecette(MySqlConnection connexion) affiche tout d'abord la liste des recettes pour permettre à l'admin de choisir quoi supprimer.
        /// Tant que le nom de recette tapé n'est pas bon, il doit en retaper un.
        /// La recette choisie est ensuite supprimée de la table cook.recette.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void SupprimerRecette(MySqlConnection connexion)
        {
            WriteLine("Voici l'ensemble des recettes");
            BaseDeDonnées.AfficherTuples(connexion, "recette");
            Clear();
            WriteLine("Tapez le nom de la recette que vous souhaitez supprimer");
            bool faux = true;
            string recette = null;
            while(faux == true)
            {
                recette = ReadLine();
                MySqlCommand command1 = connexion.CreateCommand();
                command1.Parameters.Add(new MySqlParameter("@recette", MySqlDbType.VarChar, value:recette, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                command1.CommandText = "SELECT count(*) FROM cook.recette WHERE nomRecette = @recette;";
                MySqlDataReader reader = command1.ExecuteReader();
                while (reader.Read())
                {
                    if(reader.GetInt32(0) == 1)
                    {
                        faux = false;
                    }
                    else
                    {
                        WriteLine("Veuillez taper le nom d'une recette présente dans la base de données");
                    }
                }
                reader.Close();
                command1.Dispose();
            }
            MySqlCommand command2 = connexion.CreateCommand();
            command2.Parameters.Add(new MySqlParameter("@recette", MySqlDbType.VarChar, value: recette, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            command2.CommandText = "DELETE FROM cook.recette WHERE nomRecette = @recette;";
            BaseDeDonnées.NonQuery(command2);
        }

        /// <summary>
        /// TopSemaine(MySqlConnection connexion) affiche le cdr dont la somme de toutes les commandes de recettes sur la semaine dernière 
        /// est la plus importante (entre il y a 7 jours et aujourdhui).
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void TopSemaine(MySqlConnection connexion) 
        {
            MySqlCommand command = connexion.CreateCommand();
            command.Parameters.Add(new MySqlParameter("@dateanterieure", MySqlDbType.DateTime, value: DateTime.Today.AddDays(-7), size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            command.Parameters.Add(new MySqlParameter("@aujourdhui", MySqlDbType.DateTime, value: DateTime.Today, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            command.CommandText = "SELECT cl.nomClient,r.codeCreateur,sum(k.occurrencesRecette) from cook.compositionCommande k,cook.commande c,cook.client cl,cook.recette r WHERE cl.codeClient = r.codeCreateur and c.numCommande = k.numCommande and k.nomRecette = r.nomRecette and c.dateCommande >= @dateanterieure and c.dateCommande <=@aujourdhui group by r.codeCreateur order by sum(k.occurrencesRecette) desc LIMIT 1;"; 

            MySqlDataReader reader = command.ExecuteReader();
            WriteLine("Voici le créateur ayant été le plus commandé cette semaine.");
            WriteLine();
            while (reader.Read())
            {
                WriteLine("Le cdr "+reader.GetString(0) + " de code " + reader.GetString(1) + " a été commandé "+reader.GetInt32(2) +" fois la semaine dernière. Félicitations à lui !");
            }
            reader.Close();
            command.Dispose();
        }

        /// <summary>
        /// TopCinqRecettes(MySqlConnection connexion) affiche la liste des 5 recettes dont la somme des commandes sont les plus élevées
        /// sur les 7 jours passés, triées par somme des commandes dans l'ordre décroissant.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void TopCinqRecettes(MySqlConnection connexion)
        {
            MySqlCommand command = connexion.CreateCommand();
            command.Parameters.Add(new MySqlParameter("@dateanterieure", MySqlDbType.DateTime, value: DateTime.Today.AddDays(-7), size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            command.Parameters.Add(new MySqlParameter("@aujourdhui", MySqlDbType.DateTime, value: DateTime.Today, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            command.CommandText = "SELECT r.nomRecette,cl.nomClient,sum(k.occurrencesRecette) from cook.compositionCommande k,cook.recette r, cook.client cl WHERE cl.codeClient = r.codeCreateur AND k.nomRecette = r.nomRecette  group by r.nomRecette order by sum(k.occurrencesRecette) desc LIMIT 5;";
            MySqlDataReader reader = command.ExecuteReader();
            WriteLine("Voici la liste des  recettes les plus commandées de la semaine passée et leur nombre de commandes");
            WriteLine();
            while (reader.Read())
            {
                WriteLine("La recette " + reader.GetString(0) + " imaginée par " + reader.GetString(1) + " a été commandée " + reader.GetInt32(2) + " depuis l'ouverture de Ma Petite Cuisine");
            }
            reader.Close();
            command.Dispose();
        }

        /// <summary>
        /// CdrOr(MySqlConnection connexion) récupère d'abord le createur le plus commandé de l'histoire du site.
        /// On sélectionne ensuite les 5 recettes les plus commandées par ordre décroissants du total de commandes.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void CdrOr(MySqlConnection connexion)
        {
            MySqlCommand cdrCommand = connexion.CreateCommand();
            cdrCommand.CommandText = "SELECT r.codeCreateur,c.nomClient from cook.compositionCommande k,cook.client c,cook.commande co,cook.recette r WHERE co.numCommande = k.numCommande and k.nomRecette = r.nomRecette and c.codeClient =r.codeCreateur group by r.codeCreateur order by sum(k.occurrencesRecette) desc LIMIT 1;";
            MySqlDataReader reader = cdrCommand.ExecuteReader();

            string topcdrcode = "";
            string topcdrnom = "";
            while (reader.Read())
            {
                topcdrcode = reader.GetString(0);
                topcdrnom = reader.GetString(1);
            }
            reader.Close();
            cdrCommand.Dispose();


            MySqlCommand commandeCDR = connexion.CreateCommand();
            commandeCDR.Parameters.Add(new MySqlParameter("@topcdrparam", MySqlDbType.VarChar, value: topcdrcode, size: 4, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));

            commandeCDR.CommandText = "SELECT r.nomRecette,sum(cc.occurrencesRecette) FROM cook.client c,cook.cdr cd, cook.compositionCommande  cc, cook.recette r WHERE r.nomRecette = cc.nomRecette and r.codeCreateur = cd.codeCdr and c.codeClient = cd.codeCdr and cd.codeCdr = @topcdrparam group by r.nomRecette order by sum(cc.occurrencesRecette) desc LIMIT 5;";
            WriteLine("Le Cdr d'or est décerné à " + topcdrnom + " dont les cinq recettes les plus commandées sont les suivantes:");
            reader = commandeCDR.ExecuteReader();
            WriteLine();
            while (reader.Read())
            {
                WriteLine(reader.GetString(0) + " commandée " + reader.GetString(1) + " fois depuis l'ouverture de Ma Petite Cuisine.");
            }
            reader.Close();
            commandeCDR.Dispose();


        }

        #endregion
    }
}

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace cookapp
{
    class Client //si on le supprime de la base faire en sorte qu'on le supprime du code... lier l'objet à son tuple
    {
        #region champs

        #endregion

        #region propriétés
        /// <summary>
        /// CodeClient correspond au codeClient dans la table cook.client
        /// </summary>
        public string CodeClient { get; }
        /// <summary>
        /// Password correspond à mdp dans la table cook.client
        /// </summary>
        public string Password { get; set; } 

        #endregion

        #region Constructeur
        public Client(string codeClient,string password)
        {
            this.CodeClient = codeClient;
            this.Password = password;
        }
        #endregion


        #region méthodes
        /// <summary>
        /// La méthode login est statique. En effet, on ne peut utiliser une instance d'un utilisateur s'il n'a pas encore de compte par exemple.
        /// </summary>
        public static CreateurDeRecettes Login(MySqlConnection connexion,string id,string table) // Un cdr est un client donc JE PEUX UTILISER LA METHODE POUR LES DEUX
        {
            bool connecte = false;
            string code = null;
            string mdp = null;
            while(connecte == false)
            {
                WriteLine("Veuillez entrer votre ID.");
                code = ReadLine();
                WriteLine("Veuillez entrer votre mot de passe.");
                mdp = ReadLine();
                Clear();

                MySqlCommand log = connexion.CreateCommand();
                log.Parameters.Add(new MySqlParameter("@codeC", MySqlDbType.VarChar, value: code, size: 4, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                log.Parameters.Add(new MySqlParameter("@motdepasse", MySqlDbType.VarChar, value: mdp, size: 24, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                log.CommandText = "SELECT " + id + ", mdp from cook." + table + " WHERE " + id + " = @codeC AND mdp = @motdepasse;";
                MySqlDataReader reader = log.ExecuteReader();
                while (reader.Read())
                {
                    WriteLine(reader.GetString(0) + " " + reader.GetString(1));
                    if (reader.GetString(0) == code && reader.GetString(1) == mdp)
                    {
                        connecte = true;
                    }
                }
                reader.Close();
                log.Dispose();
            }
            return new CreateurDeRecettes(code, mdp);
        }


        /// <summary>
        /// La méthode commander() a été commentée au fur et à mesure du déroulement du code
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public void Commander(MySqlConnection connexion) 
        {
            bool fincommande = false;

            // choix aléatoire d'un cuisiner pour réaliser la commande
            MySqlCommand choixcuisinier = connexion.CreateCommand(); 
            choixcuisinier.CommandText = "SELECT idCook FROM cook.cooking ORDER BY RAND() LIMIT 1;";
            MySqlDataReader lecteur = choixcuisinier.ExecuteReader();
            string cuisinier = null; 
            while (lecteur.Read()) 
            {
                cuisinier = lecteur.GetString(0);
                WriteLine("Votre commande sera cuisinée par le cuisinier: " + cuisinier);
            }
            lecteur.Dispose();
            choixcuisinier.Dispose();

            //Création du numéro de commande (il s'agit d'un compteur du nombre de commandes de l'appli)
            string numeroCommande = null; 
            MySqlCommand numCommand = connexion.CreateCommand();
            numCommand.CommandText = "SELECT COUNT(*) FROM cook.commande;";
            MySqlDataReader lecture = numCommand.ExecuteReader();
            while (lecture.Read())
            {
                numeroCommande = lecture.GetInt32(0).ToString();
            }
            numCommand.Dispose();
            lecture.Dispose();

            // Le client choisit le jour et l'heure auxquels il veut etre livré, avec des while pour éviter les erreurs de saisie
            WriteLine("Bienvenue dans le gestionnaire de commande de Ma Petite Cuisine !");
            WriteLine();
            WriteLine("Pour quel jour souhaitez-vous commander ? Veuillez taper le numéro du jour et du mois sous la forme JJ/MM");
            string day = ReadLine();
            Clear();
            int result;
            int result2;
            while (day.Length != 5 || int.TryParse(day.Substring(0, 2), out result) == false || day.Substring(2, 1) != "/" || int.TryParse(day.Substring(3, 2), out result2) == false || new DateTime(2020, Int32.Parse(day.Substring(3, 2)), Int32.Parse(day.Substring(0, 2))) < DateTime.Today)
            {
                
                WriteLine("Erreur d'entrée. Veuillez entrer la date sous la forme JJ/MM, une date qui ne soit bien sure pas déjà passée");
                day = ReadLine();
                Clear();
            }

            int result3;
            int result4;
            WriteLine("Veuillez entre l'heure à laquelle vous souhaitez etre livré(e) sous la forme HH:MM");
            string hour = ReadLine();
            Clear();
            while (hour.Length != 5 || int.TryParse(hour.Substring(0, 2), out result3) == false || hour.Substring(2, 1) != ":" || int.TryParse(hour.Substring(3, 2), out result4) == false || new DateTime(2020, Int32.Parse(day.Substring(3, 2)), Int32.Parse(day.Substring(0, 2)), Int32.Parse(hour.Substring(0, 2)), Int32.Parse(hour.Substring(3, 2)), 0) < DateTime.Today)
            {
                WriteLine("Erreur d'entrée. Veuillez entrer la date sous la forme HH/MM, une heure qui ne soit bien sure pas déjà passée");
                day = ReadLine();
                Clear();
            }
            DateTime dateLivraison = new DateTime(2020, Int32.Parse(day.Substring(3, 2)), Int32.Parse(day.Substring(0, 2))); 


            //la commande est insérée avec toutes ses caractéristiques dans la bdd
            MySqlCommand commandeCommand = connexion.CreateCommand();
            commandeCommand.Parameters.Add(new MySqlParameter("@clientparam", MySqlDbType.VarChar, value: this.CodeClient, size: 4, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            commandeCommand.Parameters.Add(new MySqlParameter("@paramnumcommande", MySqlDbType.VarChar, value: numeroCommande, size: 6, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            commandeCommand.Parameters.Add(new MySqlParameter("@datecommandeParam", MySqlDbType.DateTime, value: DateTime.Today, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            commandeCommand.Parameters.Add(new MySqlParameter("@dateLivraisonParam", MySqlDbType.DateTime, value: dateLivraison, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            commandeCommand.Parameters.Add(new MySqlParameter("@cuisinierParam", MySqlDbType.VarChar, value: cuisinier, size: 5, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            commandeCommand.CommandText = "INSERT INTO cook.commande(numCommande,codeClient,dateCommande,dateLivraison,idCook,livree) VALUES(@paramnumcommande,@clientparam,@datecommandeParam,@dateLivraisonParam,@cuisinierParam,'non');";
            BaseDeDonnées.NonQuery(commandeCommand);

            Dictionary<string, float> quantites = new Dictionary<string, float>();
            if(DateTime.Today == dateLivraison)
            {
                quantites = BaseDeDonnées.QProduitCommandes(connexion, dateLivraison,"stockActuel");
            }
            else
            {
                quantites = BaseDeDonnées.QProduitCommandes(connexion, dateLivraison, "stockMax");
            }

            // le client peut commander plusieurs plats différents
            while (fincommande == false)
            {
                List<string> recettes = new List<string>();
                MySqlCommand choixPlats = connexion.CreateCommand();
                choixPlats.Parameters.Add(new MySqlParameter("@paramnumcommande", MySqlDbType.VarChar, value: numeroCommande, size: 6, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));

                //on affiche la liste des plats disponibles (= plats n'ayant pas encore été commandés dans cette commande)
                WriteLine("Voici la liste de nos plats disponibles: ");
                WriteLine();
                choixPlats.CommandText = "SELECT r.nomRecette from cook.recette r WHERE r.nomRecette NOT IN (SELECT cc.nomRecette FROM cook.compositionCommande cc,commande c WHERE cc.numCommande = c.numCommande AND cc.numCommande = @paramnumcommande );";
                MySqlDataReader reader;
                reader = choixPlats.ExecuteReader();
                while (reader.Read())
                {
                    WriteLine(reader.GetString(0));
                    WriteLine();
                    recettes.Add(reader.GetString(0));
                }
                reader.Close();
                choixPlats.Dispose();

                //l'utilisateur saisit le nom du plat/recette choisi(e)
                WriteLine("Veuillez taper le nom du plat que vous souhaitez commander");
                string choix = ReadLine();
                WriteLine();
                while (recettes.Contains(choix) == false)
                {
                    WriteLine("Veuillez taper le nom d'un plat disponible ou que vous n'avez pas déjà commandé");
                    choix = ReadLine();
                    Clear();
                }
                Clear();
                int nombre = 0;

                // la liste occurrenceMax va stocker les produits et le nombre maximal de fois que chacun peut etre utilisé pour confectionner un
                // certain nombre de fois ce plats
                // On teste ces nombres maximaux par le while ci-dessous en les retranchant à la valeur du dictionnaire représentant le stock actualisé
                //d'un produit, c'est à dire le stock en prenant en compte toutes les autres recettes deja commandées pour la meme journée
                // L'occurrence maximale de commande représentera le nombre de fois minimal qu'un des produits peut etre utilisé
                //Ce sera ainsi le nombre de fois maximal que la recette pourra etre commandée ce jour là
                List<Produit> occurrencesMax = new List<Produit>();
                MySqlCommand produitDansNbRecettes = connexion.CreateCommand();
                produitDansNbRecettes.Parameters.Add(new MySqlParameter("@choix", MySqlDbType.VarChar, value: choix, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                produitDansNbRecettes.CommandText = "SELECT p.nomProduit,cr.quantiteProduit FROM cook.contenanceRecette cr, produit p WHERE cr.recetteNom = @choix and cr.nomProd = p.nomProduit;";
                reader = produitDansNbRecettes.ExecuteReader();
                while (reader.Read())
                {
                    int i = 0;
                    float temp = quantites[reader.GetString(0)];
                    while (temp >= 0)
                    {
                        temp -= reader.GetFloat(1);
                        i++;
                    }
                    if (temp == 0)
                    {
                        Produit produit = new Produit { NomProduit = reader.GetString(0), Occurrence = i, Quantite = reader.GetFloat(1) };
                        occurrencesMax.Add(produit);
                    }
                    else
                    {
                        Produit produit = new Produit { NomProduit = reader.GetString(0), Occurrence = i - 1, Quantite = reader.GetFloat(1) };
                        occurrencesMax.Add(produit);
                    }
                }
                reader.Close();
                produitDansNbRecettes.Dispose();
                occurrencesMax.Sort((a, b) => (a.Occurrence.CompareTo(b.Occurrence)));
                //on prend bien l'occurence minimale
                int commandemax = occurrencesMax[0].Occurrence;
                WriteLine("Vous pouvez commander ce plat au maximum " + commandemax + " fois");
                WriteLine();
                WriteLine("Tapez le nombre de fois que vous voulez commander ce plat");
                nombre = (int)Program.TryParseFloat();
                Clear();
                while (nombre > commandemax)
                {
                    WriteLine("Vous pouvez commander ce plat au maximum " + commandemax + " fois");
                    nombre = (int)Program.TryParseFloat();
                    Clear();

                }

                // On actualise les stocks pour la prochaine recette commandée
                foreach (string entry in quantites.Keys.ToArray())
                {
                    foreach (Produit prod in occurrencesMax)
                    {
                        if (entry == prod.NomProduit)
                        {
                            quantites[entry] -= prod.Quantite * nombre;
                        }

                    }
                }
            

                // On insère la composition de la commande (association commande - recette) dans la base
                MySqlCommand commandeContenance = connexion.CreateCommand();
                commandeContenance.Parameters.Add(new MySqlParameter("@paramnumcommande", MySqlDbType.VarChar, value: numeroCommande, size: 6, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                commandeContenance.Parameters.Add(new MySqlParameter("@paramRecette", MySqlDbType.VarChar, value: choix, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                commandeContenance.Parameters.Add(new MySqlParameter("@paramoccurrence", MySqlDbType.Int32, value: nombre, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                commandeContenance.CommandText = "INSERT INTO cook.compositionCommande(numCommande,nomRecette,occurrencesRecette) VALUES(@paramnumcommande,@paramRecette,@paramoccurrence);"; 
                BaseDeDonnées.NonQuery(commandeContenance);

                // On compte le nombre de fois total que la recette en question a été commandée dans l'application pour savoir si on peut en augmenter le
                //prix de vente ou non
                MySqlCommand recupoccurrence = connexion.CreateCommand();
                recupoccurrence.Parameters.Add(new MySqlParameter("@paramRecette", MySqlDbType.VarChar, value: choix, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                recupoccurrence.CommandText = "SELECT sum(occurrencesRecette) FROM cook.compositionCommande WHERE nomRecette = @paramRecette;";
                MySqlDataReader lit = recupoccurrence.ExecuteReader();
                int compteur = 0;
                while (lit.Read())
                {
                    compteur = lit.GetInt32(0);
                }
                lit.Close();
                recupoccurrence.Dispose();


                MySqlCommand updateprix = connexion.CreateCommand();
                if (compteur > 10 && compteur <= 50) 
                {

                    updateprix.CommandText = "UPDATE cook.recette  SET prix = prix + 2 WHERE nomRecette = (SELECT cc.nomRecette FROM cook.compositionCommande cc WHERE cc.nomRecette = nomRecette group by cc.nomRecette having sum(cc.occurrencesRecette) > 10 and sum(cc.occurrencesRecette) <= 50);";
                    BaseDeDonnées.NonQuery(updateprix);
                    

                }
                if (compteur > 50)
                {
                    updateprix.CommandText = "UPDATE cook.recette  SET prix = prix + 2 WHERE nomRecette = (SELECT cc.nomRecette FROM cook.compositionCommande cc WHERE cc.nomRecette = nomRecette group by cc.nomRecette having sum(cc.occurrencesRecette) > 50);";
                    BaseDeDonnées.NonQuery(updateprix);

                }


                //enfin on insère l'association entre le cuisinier et la recette dans la bdd
                MySqlCommand cuisineCommand = connexion.CreateCommand();
                cuisineCommand.Parameters.Add(new MySqlParameter("@cuisinierParam", MySqlDbType.VarChar, value: cuisinier, size: 5, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                cuisineCommand.Parameters.Add(new MySqlParameter("@paramRecette", MySqlDbType.VarChar, value: choix, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                cuisineCommand.Parameters.Add(new MySqlParameter("@dateLivraisonParam", MySqlDbType.DateTime, value: dateLivraison, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                cuisineCommand.CommandText = "INSERT INTO cook.cuisine(cookID,nomRec,datePrepa) VALUES (@cuisinierParam,@paramRecette,@dateLivraisonParam);";
                BaseDeDonnées.NonQuery(cuisineCommand);

                WriteLine("Voulez-vous poursuivre votre commande: " + "\n" + "Oui   : [O]" + "\n" + "Non  : [N]");
                bool bonneentrée = false;
                while (bonneentrée == false)
                {
                    switch (ReadKey(true).Key)
                    {
                        case ConsoleKey.N:
                            Clear(); //
                            bonneentrée = true;
                            fincommande = true;
                            break;
                        case ConsoleKey.O:
                            Clear(); //
                            bonneentrée = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            WriteLine("Merci d'avoir commandé chez MaPetiteCuisine !");
        }

        /// <summary>
        /// CreerClient(MySqlConnection connexion) est statique car ce n'est pas un client qui fait l'action, c'est le site.
        /// L'utilisateur, pour s'inscrire doit rentrer toutes ses informations (nom,mdp, numéro de telephone,adresse...).
        /// Un codeClient lui est attribué correspondant au rang d'inscription de cette personne.
        /// Le client est ainsi inséré dans la table cook.client
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public static void CreerClient(MySqlConnection connexion)
        {
            WriteLine("Veuillez entrer votre nom");
            string nom = ReadLine();
            WriteLine("Veuillez choisir un mot de passe");
            string mdp = ReadLine();
            WriteLine("Veuillez entrer votre numéro de téléphone");
            string tel = ReadLine();
            int result;
            while (int.TryParse(tel, out result) == false && tel.Length != 10)
            {
                WriteLine("Veuillez entrer un numéro de téléphone français, valide, à 10 chiffres");
                tel = ReadLine();
            }
            WriteLine("Veuillez saisir votre adresse (numéro et rue) ");
            string adresse = ReadLine();
            WriteLine("Veuillez saisir le nom de votre ville");
            string ville = ReadLine();
            string codeC = (BaseDeDonnées.CompterTuplesTable(connexion, "client") + 1).ToString();

            MySqlCommand creerClient = connexion.CreateCommand();

            creerClient.Parameters.Add(new MySqlParameter("@codeParam", MySqlDbType.VarChar, value: codeC, size: 4, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            creerClient.Parameters.Add(new MySqlParameter("@mdpParam", MySqlDbType.VarChar, value: mdp, size: 24, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            creerClient.Parameters.Add(new MySqlParameter("@telParam", MySqlDbType.VarChar, value: tel, size: 10, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            creerClient.Parameters.Add(new MySqlParameter("@nomParam", MySqlDbType.VarChar, value: nom, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            creerClient.Parameters.Add(new MySqlParameter("@adresseParam", MySqlDbType.VarChar, value: adresse, size: 50, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            creerClient.Parameters.Add(new MySqlParameter("@villeParam", MySqlDbType.VarChar, value: ville, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            creerClient.CommandText = "INSERT INTO cook.client (codeClient,mdp,numTelClient,nomClient,adresseClient,villeClient) VALUES (@codeParam,@mdpParam,@telParam,@nomParam,@adresseParam,@villeParam);";
            BaseDeDonnées.NonQuery(creerClient);

        }

        /// <summary>
        ///  ClientToCdr(MySqlConnection connexion) regarde d'abord si le client est déjà cdr (on cherche this.CodeClient dans cook.cdr,command1)
        ///  puis insère le client avec son code client et son mot de passe dans la table cook.cdr, avec un solde initialisé à 0 (command2),
        ///  s'il n'est pas encore cdr.
        ///  Sinon la méthode affiche un message pour prévenir le client qu'il est déjà cdr.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public void ClientToCdr(MySqlConnection connexion) 
        {
            MySqlCommand command1 = connexion.CreateCommand();
            command1.Parameters.Add(new MySqlParameter("@code", MySqlDbType.VarChar, value: this.CodeClient, size: 4, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
            command1.CommandText = "SELECT count(*) FROM cook.cdr WHERE codeCdr = @code";
            MySqlDataReader reader = command1.ExecuteReader();
            int estcdr = 0;
            while (reader.Read())
            {
                if(reader.GetInt32(0) != 0)
                {
                    estcdr = 1;
                }
            }
            reader.Close();
            command1.Dispose();
            if(estcdr == 0)
            {
                MySqlCommand command2 = connexion.CreateCommand();
                command2.Parameters.Add(new MySqlParameter("@code", MySqlDbType.VarChar, value: this.CodeClient, size: 4, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                command2.Parameters.Add(new MySqlParameter("@mdp", MySqlDbType.VarChar, value: this.Password, size: 24, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                command2.CommandText = "INSERT INTO cook.cdr(codeCdr,mdp,soldeCook) VALUES (@code,@mdp,0)"; 
                BaseDeDonnées.NonQuery(command2);
                WriteLine("Félicitations ! Vous pouvez maintenant créer des recettes qui seront ajoutées au catalogue Ma Petite Cuisine !");
            }
            else
            {
                WriteLine("Vous etes déjà enregistré comme créateur de recettes.");
            }

        }

        /// <summary>
        /// ListeCommandes(MySqlConnection connexion) stocke les numéros des commandes du client dans une liste "numsCommandes".
        /// Pour chaque commande, elle affiche les plats commandés et en combien d'exemplaires, et la date de la commande (command2).
        /// Le tout est realisé en deux requetes pour avoir une ligne imprimée par commande.
        /// En effet la deuxième commande affiche chaque recette pour un numéro de commande.
        /// </summary>
        /// <param name="connexion">connexion permet d'accéder à la base par des requetes commandées depuis C#</param>
        public void ListeCommandes(MySqlConnection connexion)
        {
            List<string> numsCommandes = new List<string>();

            MySqlParameter codeC = new MySqlParameter("@codeC", MySqlDbType.VarChar);
            codeC.Value = this.CodeClient;
            MySqlCommand command1 = connexion.CreateCommand();
            command1.Parameters.Add(codeC);
            command1.CommandText = "SELECT numCommande FROM cook.commande WHERE codeClient = @codeC;";
            MySqlDataReader reader = command1.ExecuteReader();
            while (reader.Read())
            {
                numsCommandes.Add(reader.GetString(0));
            }
            reader.Close();
            command1.Dispose();

            foreach(string str in numsCommandes)
            {
                MySqlCommand command2 = connexion.CreateCommand();
                command2.Parameters.Add(new MySqlParameter("@numCommande", MySqlDbType.VarChar, value: str, size: 4, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                command2.Parameters.Add(codeC);
                command2.CommandText = "SELECT cc.nomRecette,cc.occurrencesRecette,c.dateLivraison FROM cook.commande c, cook.compositionCommande cc WHERE cc.numCommande = c.numCommande AND c.codeClient = @codeC AND c.numCommande =@numCommande;";
                reader = command2.ExecuteReader();
                string infoscommande = " ";
                string date = null;
                while (reader.Read())
                {
                    date = reader.GetString(2);
                    infoscommande += reader.GetInt32(1) + " " + reader.GetString(0) + ",";

                }
                WriteLine("La commande n°"+str+" du "+date +" contient"+infoscommande);
                WriteLine();
                reader.Close();
                command2.Dispose();
            }

        }



        static bool StockSuffisant(MySqlConnection connexion) //pour aujourd'hui
        {
            bool suffisant = true;
            Dictionary<string, float> quantites = BaseDeDonnées.QProduitCommandes(connexion,DateTime.Today,"AND cr.recetteNom = @recette");

            foreach (KeyValuePair<string, float> entry in quantites)
            {

                MySqlCommand videStock = connexion.CreateCommand();
                videStock.Parameters.Add(new MySqlParameter("@produit", MySqlDbType.VarChar, value: entry.Key, size: 20, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                videStock.Parameters.Add(new MySqlParameter("@qTotale", MySqlDbType.Float, value: entry.Value, size: default, isNullable: false, direction: default, precision: default, scale: default, sourceColumn: default, sourceVersion: default));
                videStock.CommandText = "SELECT stockActuel - @qTotale FROM cook.produit WHERE nomProduit = @produit; ";

                MySqlDataReader reader = videStock.ExecuteReader();
                while (reader.Read())
                {
                    if(reader.GetFloat(0) < 0)
                    {
                        suffisant = false;
                        break;
                    }
                }
            }
            return suffisant;
        }

        #endregion
    }
}

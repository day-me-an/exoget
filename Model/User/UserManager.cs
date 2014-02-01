using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Security.Principal;
using System.Net.Mail;

namespace Exo.Exoget.Model.User
{
    public enum UserInfoType
    {
        Settings
    }

    public partial class UserManager
    {
        private readonly MySqlConnection conn;

        public UserManager(MySqlConnection conn)
        {
            this.conn = conn;
        }

        private UserInfo GetUserByColumn(string column, object value)
        {
            UserInfo user = null;

            using (MySqlCommand command = new MySqlCommand(@"
SELECT id, username, email, firstname, lastname, gender, dob, location, profilePublicFavorites, profilePublicHistory, profilePublicRated
FROM users WHERE `" + column + "` = ?value", conn))
            {
                command.Parameters.AddWithValue("?value", value);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserInfo(reader.GetUInt32(0))
                        {
                            Username = reader.GetString(1),
                            Email = reader.GetString(2),
                            FirstName = reader.GetString(3),
                            LastName = reader.GetString(4),
                            Gender = (UserInfo.UserGender)reader.GetByte(5),
                            Dob = reader.GetDateTime(6),
                            Location = reader.GetString(7),
                            FavoritesPublic = reader.GetBoolean(8),
                            HistoryPublic = reader.GetBoolean(9),
                            RatedPublic=reader.GetBoolean(10)
                        };
                    }
                }
            }

            return user;
        }

        public UserInfo GetUser(uint id)
        {
            return GetUserByColumn("id", id);
        }

        public UserInfo GetUser(string username)
        {
            return GetUserByColumn("username", username);
        }

        public UserInfo GetUserByEmail(string email)
        {
            return GetUserByColumn("email", email);
        }

        public bool CheckEmailExists(string email)
        {
            bool exists = false;

            using (MySqlCommand command = new MySqlCommand("SELECT COUNT(id) FROM users WHERE email = ?email", conn))
            {
                command.Parameters.AddWithValue("?email", email);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        exists = reader.GetInt32(0) > 0;
                }
            }

            return exists;
        }

        public bool CheckUsernameExists(string username)
        {
            bool exists = false;

            using (MySqlCommand command = new MySqlCommand("SELECT COUNT(id) FROM users WHERE username = ?username", conn))
            {
                command.Parameters.AddWithValue("?username", username);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        exists = reader.GetInt32(0) > 0;
                }
            }

            return exists;
        }

        /// <summary>
        /// Creates a user, or if the user exists, updates it
        /// </summary>
        public void Save(UserInfo user)
        {
            if (user.Id == 0)
            {
                using (MySqlCommand command = new MySqlCommand(
    @"INSERT INTO users (username, password, email, firstname, lastname, gender, dob, location) 
VALUES (?username, MD5(?password), ?email, ?firstname, ?lastname, ?gender, ?dob, ?location)", conn))
                {
                    command.Parameters.AddWithValue("?username", user.Username);
                    command.Parameters.AddWithValue("?password", user.Password);
                    command.Parameters.AddWithValue("?email", user.Email);
                    command.Parameters.AddWithValue("?firstname", user.FirstName);
                    command.Parameters.AddWithValue("?lastname", user.LastName);
                    command.Parameters.AddWithValue("?gender", (byte)user.Gender);
                    command.Parameters.AddWithValue("?dob", user.Dob);
                    command.Parameters.AddWithValue("?location", user.Location);

                    command.ExecuteNonQuery();
                    user.Id = (uint)command.LastInsertedId;
                }
            }
            else
            {
                using (MySqlCommand command = new MySqlCommand(@"
SET @password = ?password;
UPDATE users
SET password=IF(@password IS NOT NULL, MD5(@password), users.password), email=?email, firstname=?firstname, lastname=?lastname, gender=?gender, dob=?dob, location=?location, profilePublicFavorites=?favoritesPublic, profilePublicHistory=?historyPublic, profilePublicRated=?ratedPublic
WHERE id=?id;", conn))
                {
                    command.Parameters.AddWithValue("?password", user.Password);
                    command.Parameters.AddWithValue("?email", user.Email);
                    command.Parameters.AddWithValue("?firstname", user.FirstName);
                    command.Parameters.AddWithValue("?lastname", user.LastName);
                    command.Parameters.AddWithValue("?gender", (byte)user.Gender);
                    command.Parameters.AddWithValue("?dob", user.Dob);
                    command.Parameters.AddWithValue("?location", user.Location);
                    command.Parameters.AddWithValue("?favoritesPublic", user.FavoritesPublic);
                    command.Parameters.AddWithValue("?historyPublic", user.HistoryPublic);
                    command.Parameters.AddWithValue("?ratedPublic", user.RatedPublic);

                    command.Parameters.AddWithValue("?id", user.Id);

                    command.ExecuteNonQuery();
                }
            }
        }

        public IPrincipal ValidateLogin(string username, string password)
        {
            IPrincipal principal = null;

            using (MySqlCommand command = new MySqlCommand(@"
SELECT users.id, users.username
FROM users
WHERE users.username=?username
AND users.password=MD5(?password)", conn))
            {
                command.Parameters.AddWithValue("?username", username);
                command.Parameters.AddWithValue("?password", password);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        principal = new GenericPrincipal(new UserIdentity(reader.GetUInt32(0), reader.GetString(1)), new string[0]);
                }
            }

            return principal;
        }
    }
}
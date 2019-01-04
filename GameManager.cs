using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts
{
    /// <inheritdoc />
    /// <summary>
    /// This class handle most of the game behaviour
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Instance of the Gamemanager
        /// </summary>
        public static GameManager Instance;
        /// <summary>
        /// Prefab for the paddle
        /// </summary>
        private GameObject _paddle;
        /// <summary>
        /// List of all instantiated GameObjects
        /// </summary>
        public List<GameObject> Instantiated { get; private set; }
        /// <summary>
        /// Indicates if movement to the right is blocked
        /// </summary>
        public bool DisableMoveRight { get; set; }
        /// <summary>
        /// Indicates if the movement to the left is blocked
        /// </summary>
        public bool DisableMoveLeft { get; set; }
        /// <summary>
        /// Delegant for the powerup event
        /// </summary>
        /// <param name="poweerup">Powerup which should be activated</param>
        public delegate void PowerUp(Enum.Powerup poweerup);
        /// <summary>
        /// Event for activating the Powerup
        /// </summary>
        public event PowerUp PowerUpEvent;
        public int NumberOfDestructibleTiles;
        private AudioManager _audioManager;

        public List<TMP_Text> GameOverScoresUi = new List<TMP_Text>();

        //Playstats
        private int _score;
        /// <summary>
        /// The amount of Lives the player has
        /// </summary>
        public int Lives;

        //Prefabs
        /// <summary>
        /// Prefab for the ball
        /// </summary>
        public GameObject BallPrefab;
        /// <summary>
        /// Prefab for the paddle
        /// </summary>
        public GameObject PaddlePrefab;
        /// <summary>
        /// Prefab for the powerups
        /// </summary>
        public GameObject PowerupPrefab;

        //Gameconfig
        /// <summary>
        /// Speed of the ball
        /// </summary>
        public int BallSpeed;
        /// <summary>
        /// Speed of the paddle
        /// </summary>
        public int PaddleSpeed;
        /// <summary>
        /// Dropchance of the powerup
        /// </summary>
        public int PowerupDropChance;

        //UI
        /// <summary>
        /// Textelement for the highscore
        /// </summary>
        public TMP_Text Highscore;
        /// <summary>
        /// Gameobject for the GameoverScreen
        /// </summary>
		public GameObject GameOverMenu;
        /// <summary>
        /// Textelement for the score at the end
        /// </summary>
		public TMP_Text FinalScore;
        /// <summary>
        /// Textelement for the amount of Lives left
        /// </summary>
        public TMP_Text LiveText;
        /// <summary>
        /// Gameobject for the PauseMenu
        /// </summary>
        public GameObject PauseMenu;

        /// <summary>
        /// Awake is called when the Instance is being loaded
        /// </summary>
        [UsedImplicitly]
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("More than one GameManager in scene!");
                return;
            }
            Instance = this;
            _audioManager = AudioManager.Instance;
            Instantiated = new List<GameObject>();
        }

        /// <summary>
        ///  Update is called once per frame
        /// </summary>
        [UsedImplicitly]
        private void Update()
        {
            if (NumberOfDestructibleTiles > 0) return;
            foreach (var ballsandpaddles in Instantiated)
            {
                Destroy(ballsandpaddles);
            }
            var powerUps = GameObject.FindGameObjectsWithTag("PowerUp");

            foreach (var powerUp in powerUps)
            {
                Destroy(powerUp);
            }
            Instantiated.Clear();
            SpawnPaddleAndBall();
            GetComponent<LevelSpawner>().SpawnNextLevel();
        }

        /// <summary>
        /// A method to call the level directly with an id
        /// </summary>
        /// <param name="id">id of the level that should be spawned </param>
        public void SpawnLevel(int id)
        {
            var level = GameObject.Find("Level");
            Destroy(level);
            foreach (var ballsandpaddles in Instantiated)
            {
                Destroy(ballsandpaddles);
            }
            var powerUps = GameObject.FindGameObjectsWithTag("PowerUp");

            foreach (var powerUp in powerUps)
            {
                Destroy(powerUp);
            }
            Instantiated.Clear();
            SpawnPaddleAndBall();
            if (id == 8)
            {
                GetComponent<LevelSpawner>().LoadLevel(GetComponent<LevelSpawner>().GenerateRandomLevel(5, 2, 5));
            }
            else
            {
                GetComponent<LevelSpawner>().LoadLevel(id);
            }
        }

        /// <summary>
        /// Method to activate or deactivate the ballcatcher graphic
        /// </summary>
        /// <param name="activate">Bool to activate or deactivate the ballcatcher</param>
        public void TogglePaddleCatcher(bool activate)
        {
            _paddle.GetComponent<PaddleController>().ActivateCatcher(activate);
        }

        /// <summary>
        /// Method to add points to the score
        /// </summary>
        /// <param name="additionalScore">The amount of points which gets added</param>
        /// <returns>The new score</returns>
        public int AddScore(int additionalScore)
        {
            _score += additionalScore;
            Highscore.text = "" + _score;
            return _score;
        }

        public void AddLive()
        {
            Lives++;
            LiveText.text = "Lives: " + Lives;
        }

        /// <summary>
        /// Method to remove a life
        /// </summary>
        public void RemoveLife()
        {
            Lives -= 1;
            if (Lives <= 0)
            {
				OnGameOver ();
            }
            else
            {
                _audioManager.Play("BallOutSound");
                SpawnPaddleAndBall();
            }
            LiveText.text = "Lives: " + Lives;
        }

        public void OnPause()
        {
            Time.timeScale = 0;
            PauseMenu.SetActive(true);
        }

        public void OnUnpause()
        {
            Time.timeScale = 1;
            PauseMenu.SetActive(false);
        }

        /// <summary>
        /// Method to activate GameOver
        /// </summary>
		public void OnGameOver(){
            _audioManager.Play("GameOverSound");
           
			Time.timeScale = 0;
            SetupGameOverMenu();
			GameOverMenu.SetActive (true);
		}
        /// <summary>
        /// Set all texts for the gameoverUI
        /// Save highscores
        /// </summary>
        private void SetupGameOverMenu()
        {
            SaveLoad.SaveNewHighscore(_score);
            var highscores = SaveLoad.LoadHighscores();
            for (var i = 0; i < 10; i++)
            {
                GameOverScoresUi[i].text = highscores[i].ToString();
            }
            var finalScoreText = "";
            for (var i = 6; i > _score.ToString().Length; i--)
            {
                finalScoreText += "0";
            }
            FinalScore.text = finalScoreText + _score;

            GameOverMenu.SetActive(true);
        }

		/// <summary>
        /// Spawns the ball and the paddle at the starting position
        /// </summary>
        public void SpawnPaddleAndBall()
        {
            _paddle = Instantiate(PaddlePrefab, new Vector3(0, -4.5f, 0), transform.rotation);
            Instantiated.Add(_paddle);
            var ball = CreateBallAt(new Vector3(0, -4.1f, 0));
            ball.GetComponent<BallPhysics>().BallBoundToPaddle = true;
            Instantiated.Add(ball);
        }

        /// <summary>
        /// Function to create a new ball
        /// </summary>
        /// <param name="position">Position where the new ball will be spawned</param>
        /// <returns>The new created ball</returns>
        public GameObject CreateBallAt(Vector3 position)
        {
            return Instantiate(BallPrefab, position, transform.rotation);
        }

        /// <summary>
        /// Function to create a new Powerup
        /// </summary>
        /// <param name="position">Position where the new powerup will be spawned</param>
        /// <returns>The new created powerup</returns>
        public GameObject CreatePowerup(Vector3 position)
        {
            var powerup = Instantiate(PowerupPrefab, position, transform.rotation);
            var random = new System.Random();
            switch (random.Next(3))
            {
                case 0:
                    powerup.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/M");
                    powerup.GetComponent<Powerup>().PowerupEnum = Enum.Powerup.Multiball;
                    break;
                case 1:
                    powerup.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/B");
                    powerup.GetComponent<Powerup>().PowerupEnum = Enum.Powerup.Breakthrough;
                    break;
                case 2:
                    powerup.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/L");
                    powerup.GetComponent<Powerup>().PowerupEnum = Enum.Powerup.ExtraLive;
                    break;
                default:
                    return null;
            }
            return powerup;
        }

        /// <summary>
        /// Method to notify all subscriber to activate a powerup
        /// </summary>
        /// <param name="powerup">The powerup which should be activated</param>
        public void Notify(Enum.Powerup powerup)
        {
            if (PowerUpEvent == null) return;
            if (powerup == Enum.Powerup.ExtraLive)
            {
                AddLive();
            }
            else
            {
                PowerUpEvent(powerup);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using SixteenBit.Generative;

namespace SixteenBit.Gameplay
{
    public enum GameState
    {
        TitleScreen,
        Loading,
        Playing,
        Paused,
        LevelComplete
    }

    /// <summary>
    /// Singleton game manager. Controls game state, level progression, and
    /// coordinates all major systems. Auto-creates itself via RuntimeInitializeOnLoadMethod.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.TitleScreen;
        public int CurrentDifficulty { get; set; } = 0;
        public int CurrentEra { get; set; } = 0;
        public int Score { get; set; }
        public LevelData CurrentLevel { get; set; }
        public LevelID CurrentLevelID { get; set; }

        private LevelLoader _levelLoader;
        private GameObject _titleScreenObj;
        private GameObject _hudObj;
        private GameObject _pauseMenuObj;
        private GameObject _levelCompleteObj;
        private GameObject _touchControlsObj;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            if (Instance != null) return;
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _levelLoader = gameObject.AddComponent<LevelLoader>();

            // Ensure EventSystem exists for UI interaction
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
                DontDestroyOnLoad(esGO);
            }
        }

        private void Start()
        {
            TransitionTo(GameState.TitleScreen);
        }

        private void Update()
        {
            // Read keyboard input every frame
            InputManager.UpdateKeyboard();

            switch (CurrentState)
            {
                case GameState.Playing:
                    if (InputManager.PausePressed)
                        PauseGame();
                    break;
                case GameState.Paused:
                    if (InputManager.PausePressed)
                        ResumeGame();
                    break;
            }

            // Clear single-frame flags at end of frame
            InputManager.LateReset();
        }

        public void TransitionTo(GameState newState)
        {
            // Cleanup previous state UI
            DestroyUI();

            CurrentState = newState;
            InputManager.Clear();

            switch (newState)
            {
                case GameState.TitleScreen:
                    Time.timeScale = 1f;
                    _levelLoader.CleanupLevel();
                    CreateTitleScreen();
                    break;
                case GameState.Loading:
                    StartLevel();
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    CreateHUD();
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    CreatePauseMenu();
                    break;
                case GameState.LevelComplete:
                    Time.timeScale = 1f;
                    CreateLevelComplete();
                    break;
            }
        }

        public void StartGame()
        {
            Score = 0;
            TransitionTo(GameState.Loading);
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing) return;
            TransitionTo(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;
            DestroyUI();
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            CreateHUD();
        }

        public void CompleteLevel()
        {
            if (CurrentState != GameState.Playing) return;
            TransitionTo(GameState.LevelComplete);
        }

        public void RestartLevel()
        {
            TransitionTo(GameState.Loading);
        }

        public void ReturnToTitle()
        {
            TransitionTo(GameState.TitleScreen);
        }

        public void NextLevel()
        {
            // Advance era every 5 levels, cycle difficulty
            CurrentDifficulty = (CurrentDifficulty + 1) % 4;
            if (CurrentDifficulty == 0)
                CurrentEra = Mathf.Min(CurrentEra + 1, 9);
            TransitionTo(GameState.Loading);
        }

        private void StartLevel()
        {
            CurrentLevelID = LevelID.GenerateNew(CurrentDifficulty, CurrentEra);
            _levelLoader.LoadLevel(CurrentLevelID);
            TransitionTo(GameState.Playing);
        }

        private void DestroyUI()
        {
            if (_titleScreenObj) Destroy(_titleScreenObj);
            if (_hudObj) Destroy(_hudObj);
            if (_pauseMenuObj) Destroy(_pauseMenuObj);
            if (_levelCompleteObj) Destroy(_levelCompleteObj);
            if (_touchControlsObj) Destroy(_touchControlsObj);
        }

        private void CreateTitleScreen()
        {
            _titleScreenObj = new GameObject("TitleScreenUI");
            _titleScreenObj.AddComponent<SixteenBit.UI.TitleScreenUI>();
        }

        private void CreateHUD()
        {
            _hudObj = new GameObject("GameplayHUD");
            _hudObj.AddComponent<SixteenBit.UI.GameplayHUD>();

            _touchControlsObj = new GameObject("TouchControls");
            _touchControlsObj.AddComponent<SixteenBit.UI.TouchControlsUI>();
        }

        private void CreatePauseMenu()
        {
            _pauseMenuObj = new GameObject("PauseMenuUI");
            _pauseMenuObj.AddComponent<SixteenBit.UI.PauseMenuUI>();
        }

        private void CreateLevelComplete()
        {
            _levelCompleteObj = new GameObject("LevelCompleteUI");
            _levelCompleteObj.AddComponent<SixteenBit.UI.LevelCompleteUI>();
        }
    }
}

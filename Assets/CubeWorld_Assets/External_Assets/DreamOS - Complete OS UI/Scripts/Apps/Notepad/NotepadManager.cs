using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    public class NotepadManager : MonoBehaviour
    {
        //Resources
        public NotepadLibrary libraryAsset;
        [SerializeField] private Transform noteLibraryParent;
        [SerializeField] private GameObject noteLibraryButton;
        public GameObject notepadWindow;
        [SerializeField] private Animator viewerAnimator;
        [SerializeField] private TMP_InputField viewerTitle;
        [SerializeField] private TMP_InputField viewerContent;
        [SerializeField] private Button deleteButton;
        public NotepadStoring notepadStoring;

        // Settings
        public bool sortListByName = true;
        public bool saveCustomNotes = false;
        [HideInInspector] public int currentNoteIndex;
        [HideInInspector] public bool modSupport;

        void Awake()
        {
            if (modSupport == true)
                return;

            InitializeNotes();
        }

        void OnDestroy()
        {
            for (int i = 0; i < libraryAsset.notes.Count; ++i)
            {
                if (libraryAsset.notes[i].isCustom == true)
                    libraryAsset.notes.RemoveAt(i);
            }
        }

        public void InitializeNotes()
        {
            PrepareNotes();
            if (notepadStoring != null) { notepadStoring.ReadNoteData(); }
        }

        void PrepareNotes()
        {
            // Delete every note in library parent
            foreach (Transform child in noteLibraryParent) { Destroy(child.gameObject); }

            // Instantiate the entire note library as buttons
            for (int i = 0; i < libraryAsset.notes.Count; ++i)
            {
                // Checking for mods
                if (libraryAsset.notes[i].isModContent == true && libraryAsset.notes[i].modHelper == true)
                {
                    libraryAsset.notes.RemoveAt(i);
                    i--; continue;
                }

                if (libraryAsset.notes[i].isDeleted == false && libraryAsset.notes[i].isCustom == false)
                {
                    // Spawn note button
                    GameObject go = Instantiate(noteLibraryButton, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    go.transform.SetParent(noteLibraryParent, false);
                    go.gameObject.name = libraryAsset.notes[i].noteTitle;

                    // Set ID tags
                    TextMeshProUGUI noteTitleObj;
                    noteTitleObj = go.transform.Find("Note Title").GetComponent<TextMeshProUGUI>();
                    noteTitleObj.text = libraryAsset.notes[i].noteTitle;

                    // Add button events
                    Button itemButton;
                    itemButton = go.GetComponent<Button>();
                    itemButton.onClick.AddListener(delegate
                    {
                        OpenNote(go.transform.GetSiblingIndex());
                        viewerAnimator.Play("In");
                        currentNoteIndex = go.transform.GetSiblingIndex();

                        deleteButton.onClick.RemoveAllListeners();
                        deleteButton.onClick.AddListener(delegate
                        {
                            libraryAsset.notes[go.transform.GetSiblingIndex()].isDeleted = true;
                            go.SetActive(false);
                            viewerAnimator.Play("Out");
                        });
                    });

                    // If it's deleted, make it disabled
                    if (libraryAsset.notes[i].isDeleted == true)
                        go.SetActive(false);
                }

                else if (libraryAsset.notes[i].isDeleted == true)
                {
                    libraryAsset.notes.RemoveAt(i);
                    i--; continue;
                }
            }
        }

        public void OpenNote(int noteIndex)
        {
            // Open note depending on note index from the library
            viewerTitle.text = libraryAsset.notes[noteIndex].noteTitle;
            viewerContent.text = libraryAsset.notes[noteIndex].noteContent;
            viewerContent.enabled = false;
            viewerContent.enabled = true;
        }

        public void OpenCustomNote(string title, string note)
        {
            // Create and open note depending on reference
            NotepadLibrary.NoteItem item = new NotepadLibrary.NoteItem();
            item.noteTitle = title;
            item.noteContent = note;
            item.isDeleted = true;
            libraryAsset.notes.Add(item);
            OpenNote(libraryAsset.notes.Count - 1);
            currentNoteIndex = libraryAsset.notes.Count - 1;
            libraryAsset.notes.RemoveAt(libraryAsset.notes.Count - 1);
        }

        public void CreateNote()
        {
            // Add a new note item to the library
            NotepadLibrary.NoteItem newNote = new NotepadLibrary.NoteItem
            {
                noteTitle = "New note",
                noteContent = "My note content",
                isCustom = true
            };

            libraryAsset.notes.Add(newNote);

            // Instantiate the note as button
            GameObject go = Instantiate(noteLibraryButton, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            go.transform.SetParent(noteLibraryParent, false);
            go.gameObject.name = libraryAsset.notes[libraryAsset.notes.Count - 1].noteTitle;

            // Set ID tags
            TextMeshProUGUI noteTitleObj;
            noteTitleObj = go.transform.Find("Note Title").GetComponent<TextMeshProUGUI>();
            noteTitleObj.text = libraryAsset.notes[libraryAsset.notes.Count - 1].noteTitle;

            // Add button events
            Button itemButton;
            itemButton = go.GetComponent<Button>();
            itemButton.onClick.AddListener(delegate
            {
                OpenNote(go.transform.GetSiblingIndex());
                viewerAnimator.Play("In");
                currentNoteIndex = go.transform.GetSiblingIndex();

                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(delegate
                {
                    libraryAsset.notes[go.transform.GetSiblingIndex()].isDeleted = true;
                    go.SetActive(false);
                    viewerAnimator.Play("Out");
                });
            });

            // Set the current note index and invoke the events
            currentNoteIndex = go.transform.GetSiblingIndex();
            itemButton.onClick.Invoke();
        }

        public void UpdateNote()
        {
            // Update the note content if it's not deleted
            if (currentNoteIndex <= libraryAsset.notes.Count - 1 && libraryAsset.notes[currentNoteIndex].isDeleted == false)
            {
                libraryAsset.notes[currentNoteIndex].noteTitle = viewerTitle.text;
                libraryAsset.notes[currentNoteIndex].noteContent = viewerContent.text;

                TextMeshProUGUI buttonTitle;
                buttonTitle = noteLibraryParent.GetChild(currentNoteIndex).GetComponentInChildren<TextMeshProUGUI>();
                buttonTitle.text = viewerTitle.text;

                if (notepadStoring != null) { notepadStoring.UpdateData(); }
            }
        }

        public void CreateStoredNote(string title, string content)
        {
            // Add a new note item to the library
            NotepadLibrary.NoteItem newNote = new NotepadLibrary.NoteItem
            {
                noteTitle = title,
                noteContent = content,
                isCustom = true
            };

            libraryAsset.notes.Add(newNote);

            // Instantiate the note as button
            GameObject go = Instantiate(noteLibraryButton, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            go.transform.SetParent(noteLibraryParent, false);
            go.gameObject.name = libraryAsset.notes[libraryAsset.notes.Count - 1].noteTitle;

            // Set ID tags
            TextMeshProUGUI noteTitleObj;
            noteTitleObj = go.transform.Find("Note Title").GetComponent<TextMeshProUGUI>();
            noteTitleObj.text = libraryAsset.notes[libraryAsset.notes.Count - 1].noteTitle;

            // Add button events
            Button itemButton;
            itemButton = go.GetComponent<Button>();
            itemButton.onClick.AddListener(delegate
            {
                OpenCustomNote(title, content);

                if (viewerAnimator.gameObject.activeInHierarchy == true)
                    viewerAnimator.Play("In");

                currentNoteIndex = go.transform.GetSiblingIndex();

                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(delegate
                {
                    Debug.Log(go.transform.GetSiblingIndex());
                    libraryAsset.notes.RemoveAt(go.transform.GetSiblingIndex());
                    go.SetActive(false);
                    viewerAnimator.Play("Out");

                    if (notepadStoring != null)
                        notepadStoring.UpdateData();
                });
            });

            // Set the current note index and invoke the events
            currentNoteIndex = go.transform.GetSiblingIndex();
            itemButton.onClick.Invoke();
        }

        public void ShowNoteOnEnable()
        {
            // If there's at least one note in the library, then open it
            if (libraryAsset.notes.Count >= 1 && libraryAsset.notes[0].isDeleted == false)
            {
                OpenNote(0);
                viewerAnimator.Play("In");
            }

            else { viewerAnimator.Play("Out"); }
        }
    }
}
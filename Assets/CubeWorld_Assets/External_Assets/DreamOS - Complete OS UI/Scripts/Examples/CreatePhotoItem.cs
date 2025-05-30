//This asset was uploaded by https://unityassetcollection.com

using UnityEngine;

namespace Michsky.DreamOS.Examples
{
    [AddComponentMenu("DreamOS/Apps/Photo Gallery/Create Photo Item")]
    public class CreatePhotoItem : MonoBehaviour
    {
        [Header("Content")]
        public Sprite photoSprite;
        public string title;
        public string description;

        [Header("Resources")]
        public PhotoGalleryManager photoManager;

        public void AddPhotoToLibrary()
        {
            PhotoGalleryLibrary.PictureItem item = new PhotoGalleryLibrary.PictureItem();
            item.pictureSprite = photoSprite;
            item.pictureTitle = title;
            item.pictureDescription = description;

            if (photoManager == null)
            {
                try
                {
                    var pmObj = (PhotoGalleryManager)GameObject.FindObjectsOfType(typeof(PhotoGalleryManager))[0];
                    photoManager = pmObj;
                }

                catch 
                {
                    Debug.LogWarning("Photo Manager is not assigned.", this);
                    return;
                }
            }

            photoManager.libraryAsset.pictures.Add(item);
            photoManager.InitializePhotos();
        }

        public void AddCustomPhotoToLibrary(Sprite photoVar, string titleVar, string descriptionVar)
        {
            PhotoGalleryLibrary.PictureItem item = new PhotoGalleryLibrary.PictureItem();
            item.pictureSprite = photoVar;
            item.pictureTitle = titleVar;
            item.pictureDescription = descriptionVar;

            if (photoManager == null)
            {
                try
                {
                    var pmObj = (PhotoGalleryManager)GameObject.FindObjectsOfType(typeof(PhotoGalleryManager))[0];
                    photoManager = pmObj;
                }

                catch
                {
                    Debug.LogWarning("Photo Manager is not assigned.", this);
                    return;
                }
            }

            photoManager.libraryAsset.pictures.Add(item);
            photoManager.InitializePhotos();
        }
    }
}
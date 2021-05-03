using UnityEngine;
using System.Collections;

namespace Ps2D
{

    /// <summary>
    /// Creates the collider for the sprites.
    /// </summary>
    public class ColliderCreator
    {
        /// <summary>
        /// Create the collider for this layout and GameObject.
        /// </summary>
        /// <param name="layout">The layout.</param>
        /// <param name="root">The root game object.</param>
        public static void Create(Layout layout, GameObject root)
        {
            ColliderCreator creator = new ColliderCreator();
            creator.layout = layout;
            creator.ApplyRootCollider(root);
        }

        /// <summary>
        /// The layout to use.
        /// </summary>
        public Layout layout { get; set; }

        /// <summary>
        /// Add an optional collider to the root.
        /// </summary>
        /// <param name="root">The root</param>
        public void ApplyRootCollider(GameObject root)
        {
            // sanity
            if (layout == null || root == null || layout.rootCollider == ColliderStyle.None) return;

            // are we a sprite?
            bool isSprite = root.GetComponent<SpriteRenderer>() != null;

            // grab the size of the collider
            PixelBounds bounds = layout.trimToSprites ? layout.trimmedSpriteBounds : layout.document.bounds;
            Vector2 colliderSize = new Vector2(bounds.width * layout.coordinatesScale * (1f / layout.pixelsToUnits), bounds.height * layout.coordinatesScale * (1f / layout.pixelsToUnits));

            // grab the center of the collider
            Vector2 colliderCenter = new Vector2(bounds.width, bounds.height);
            switch (layout.anchor)
            {
                case TextAnchor.LowerCenter:
                    colliderCenter.x = colliderCenter.x * 0f;
                    colliderCenter.y = colliderCenter.y * 0.5f;
                    break;
                case TextAnchor.LowerLeft:
                    colliderCenter.x = colliderCenter.x * 0.5f;
                    colliderCenter.y = colliderCenter.y * 0.5f;
                    break;
                case TextAnchor.LowerRight:
                    colliderCenter.x = colliderCenter.x * -0.5f;
                    colliderCenter.y = colliderCenter.y * 0.5f;
                    break;
                case TextAnchor.MiddleCenter:
                    colliderCenter.x = colliderCenter.x * 0f;
                    colliderCenter.y = colliderCenter.y * 0f;
                    break;
                case TextAnchor.MiddleLeft:
                    colliderCenter.x = colliderCenter.x * 0.5f;
                    colliderCenter.y = colliderCenter.y * 0f;
                    break;
                case TextAnchor.MiddleRight:
                    colliderCenter.x = colliderCenter.x * -0.5f;
                    colliderCenter.y = colliderCenter.y * 0f;
                    break;
                case TextAnchor.UpperCenter:
                    colliderCenter.x = colliderCenter.x * 0f;
                    colliderCenter.y = colliderCenter.y * -0.5f;
                    break;
                case TextAnchor.UpperLeft:
                    colliderCenter.x = colliderCenter.x * 0.5f;
                    colliderCenter.y = colliderCenter.y * -0.5f;
                    break;
                case TextAnchor.UpperRight:
                    colliderCenter.x = colliderCenter.x * -0.5f;
                    colliderCenter.y = colliderCenter.y * -0.5f;
                    break;
                default:
                    break;
            }
            colliderCenter = new Vector2(colliderCenter.x * layout.coordinatesScale * (1f / layout.pixelsToUnits), colliderCenter.y * layout.coordinatesScale * (1f / layout.pixelsToUnits));

            // check for existing box colliders
            BoxCollider existingBoxCollider = root.GetComponent<BoxCollider>();
            BoxCollider2D existingBoxCollider2D = root.GetComponent<BoxCollider2D>();

            // do we have an existing box collider?
            if (existingBoxCollider != null || existingBoxCollider2D != null)
            {
                // refresh the BoxCollider size
                if (existingBoxCollider != null)
                {
                    existingBoxCollider.size = new Vector3(colliderSize.x, colliderSize.y, existingBoxCollider.size.z);
                    existingBoxCollider.center = new Vector3(colliderCenter.x, colliderCenter.y, existingBoxCollider.center.z);
                }

                // refresh the BoxCollider2D size
                if (existingBoxCollider2D != null)
                {
                    existingBoxCollider2D.size = colliderSize;
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
                    existingBoxCollider2D.center = colliderCenter;
#else
                    existingBoxCollider2D.offset = colliderCenter;
#endif
                }

                // we're done.
                return;
            }

            // check for existing colliders
            if (root.GetComponent<Collider2D>() != null || root.GetComponent<Collider>() != null)
            {
                Debug.LogWarning("Ps2D detected conflicting colliders on the root GameObject.  Skipping collider creation.");
                return;
            }

            // check for conflicting rigid bodies
            bool conflictRigidBody = root.GetComponent<Rigidbody2D>() != null && layout.rootCollider == ColliderStyle.BoxCollider;
            bool conflictRigidBody2D = root.GetComponent<Rigidbody>() != null && layout.rootCollider == ColliderStyle.BoxCollider2D;

            // conflict detected
            if (conflictRigidBody || conflictRigidBody2D)
            {
                Debug.LogWarning("Ps2D detected conflicting rigid bodies.  Skipping collider creation.");
                return;
            }

            // add the new collider safely
            try
            {
                // 2D collider
                if (layout.rootCollider == ColliderStyle.BoxCollider2D)
                {
                    var collider = root.AddComponent<BoxCollider2D>();

                    // configure this collider unless this is a sprite renderer (in which case we get bounds for free)
                    if (!isSprite)
                    {
                        collider.size = colliderSize;
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
                        collider.center = colliderCenter;
#else
                        collider.offset = colliderCenter;
#endif
                        collider.isTrigger = layout.rootColliderTrigger;
                    }
                }
                else
                {
                    // 3D collider
                    var collider = root.AddComponent<BoxCollider>();

                    // configure this collider unless this is a sprite renderer (in which case we get bounds for free)
                    if (!isSprite)
                    {
                        collider.size = new Vector3(colliderSize.x, colliderSize.y, 0.2f);
                        collider.center = colliderCenter;
                        collider.isTrigger = layout.rootColliderTrigger;
                    }
                }
            }
            catch
            {
                Debug.LogWarning("Error setting the collider on Ps2D root object.  Perhaps there's a conflicting component?");
            }

        }


    }

}

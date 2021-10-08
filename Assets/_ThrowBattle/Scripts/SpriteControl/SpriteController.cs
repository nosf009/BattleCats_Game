using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace _ThrowBattle
{
    public struct TextureParameter
    {
        public float widthWorld, heightWorld;
        public int widthPixel, heightPixel;
    }

    public class SpriteController : MonoBehaviour
    {
        public static SpriteController Instance { get; private set; }
        public GameObject[] birdPrefabs;
        private Dictionary<GameObject, TextureParameter> playerTexture=new Dictionary<GameObject, TextureParameter>();
        private Color transp;

        public GameObject RandomBird()
        {
            return birdPrefabs[Random.Range(0, birdPrefabs.Length)];
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                DestroyImmediate(Instance.gameObject);
                Instance = this;
            }
        }

        public void CreateCloneTexture(SpriteRenderer spriteRender)
        {
            Texture2D textureClone = (Texture2D)Instantiate(spriteRender.sprite.texture);
            spriteRender.sprite = Sprite.Create(textureClone,
                          new Rect(0f, 0f, textureClone.width, textureClone.height),
                          new Vector2(0.5f, 0.5f), 100f);
            transp = new Color(0f, 0f, 0f, 0f);
            InitSpriteDimensions(spriteRender);
        }

        private void InitSpriteDimensions(SpriteRenderer spriteRender)
        {
            TextureParameter newTexture = new TextureParameter();
            newTexture.widthWorld = spriteRender.bounds.size.x;
            newTexture.heightWorld = spriteRender.bounds.size.y;
            newTexture.widthPixel = spriteRender.sprite.texture.width;
            newTexture.heightPixel = spriteRender.sprite.texture.height;
            playerTexture.Add(spriteRender.gameObject, newTexture);
        }

        void Update()
        {

        }

        public void DestroySprite(CircleCollider2D cc,SpriteRenderer spriteRender, float dirX)
        {
            if (spriteRender)
            {
                if (!playerTexture.ContainsKey(spriteRender.gameObject))
                    CreateCloneTexture(spriteRender);

                V2int c = World2Pixel(cc.bounds.center.x, cc.bounds.center.y, spriteRender.gameObject, spriteRender, dirX);

                int r = Mathf.RoundToInt(cc.bounds.size.x * playerTexture[spriteRender.gameObject].widthPixel / playerTexture[spriteRender.gameObject].widthWorld);

                int x, y, px, nx, py, ny, d = 0;

                for (x = 0; x <= r; x++)
                {
                    bool red = false;
                    d = (int)Mathf.RoundToInt(Mathf.Sqrt(r * r - x * x));

                    if (r - x < 4)
                        red = true;
                    for (y = 0; y <= d; y++)
                    {
                        if (d - y < 4)
                            red = true;

                        px = c.x + x;
                        nx = c.x - x;
                        py = c.y + y;
                        ny = c.y - y;

                        if (red)
                        {
                            if (spriteRender.sprite.texture.GetPixel(px, py) != transp)
                                spriteRender.sprite.texture.SetPixel(px, py, Color.red);

                            if (spriteRender.sprite.texture.GetPixel(nx, py) != transp)
                                spriteRender.sprite.texture.SetPixel(nx, py, Color.red);

                            if (spriteRender.sprite.texture.GetPixel(px, ny) != transp)
                                spriteRender.sprite.texture.SetPixel(px, ny, Color.red);

                            if (spriteRender.sprite.texture.GetPixel(nx, ny) != transp)
                                spriteRender.sprite.texture.SetPixel(nx, ny, Color.red);
                        }
                        else
                        {
                            spriteRender.sprite.texture.SetPixel(px, py, transp);
                            spriteRender.sprite.texture.SetPixel(nx, py, transp);
                            spriteRender.sprite.texture.SetPixel(px, ny, transp);
                            spriteRender.sprite.texture.SetPixel(nx, ny, transp);
                        }
                    }
                }
                spriteRender.sprite.texture.Apply();
                cc.gameObject.SetActive(false);
            }
           
        }

        private V2int World2Pixel(float x, float y,GameObject textureID,SpriteRenderer spriteRenderer)
        {
            V2int v = new V2int();

            float dx = x - spriteRenderer.gameObject.transform.position.x;
            v.x = Mathf.RoundToInt(0.5f * playerTexture[textureID].widthPixel + dx * playerTexture[textureID].widthPixel / playerTexture[textureID].widthWorld);

            float dy = y - spriteRenderer.gameObject.transform.position.y;
            v.y = Mathf.RoundToInt(0.5f * playerTexture[textureID].heightPixel + dy * playerTexture[textureID].heightPixel / playerTexture[textureID].heightWorld);

            return v;
        }

        private V2int World2Pixel(float x, float y, GameObject textureID, SpriteRenderer spriteRenderer, float dirX)
        {
            V2int v = new V2int();

            float dx = x - spriteRenderer.gameObject.transform.position.x;
            v.x = Mathf.RoundToInt(0.5f * playerTexture[textureID].widthPixel + dirX * dx * playerTexture[textureID].widthPixel / playerTexture[textureID].widthWorld);

            float dy = y - spriteRenderer.gameObject.transform.position.y;
            v.y = Mathf.RoundToInt(0.5f * playerTexture[textureID].heightPixel + dy * playerTexture[textureID].heightPixel / playerTexture[textureID].heightWorld);

            return v;
        }
    }
}

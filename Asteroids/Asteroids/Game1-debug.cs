using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Asteroids
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const bool MOUSE_VISIBLE = false;
        const bool FULLSCREEN = false;
        const bool DEBUG = true;

        const int WIDTH = 950;
        const int HEIGHT = 700;
        const int MISSILE_REFRESH = 0; //milliseconds between missiles
        const int NUM_ASTEROIDS = 15;

        const int SHIP_COLLISION_OFFSET = 10;
        const float SHIP_SCALE = 0.15f;
        const float SHIP_SPEED = 1.5f;

        const int MISSILE_COLLISION_OFFSET = 5;
        const float MISSILE_SCALE = 0.10f;
        const float MISSILE_SPEED = 2.0f;

        const int ASTEROID_COLLISION_OFFSET = 5;

        Vector2 PROJECTILE_OFFSET;
        Vector2 SHIP_OFFSET;
        Vector2 SHIP_POSITION;
        Vector2 ASTEROID_OFFSET;

        readonly Color FONT_COLOR = Color.Yellow;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        Texture2D earth;
        Texture2D stars;
        Texture2D explosion;
        Texture2D missile;
        Texture2D particle;
        Texture2D shuttle;
        Texture2D asteroid;

        SoundEffect explode;
        SoundEffect gunshot;
        SoundEffect impact;

        Explosion explosionEffect;

        UserControlledSprite ship;
        List<ProjectileSprite> projectiles;
        List<Texture2D> particles;
        List<BouncingSprite> asteroids;
        Random rand;
        int missileFireTime = 0; //Time in milliseconds when the last missile was fired
        bool lost = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.IsFullScreen = FULLSCREEN;
            this.IsMouseVisible = MOUSE_VISIBLE;
            graphics.PreferredBackBufferHeight = HEIGHT;
            graphics.PreferredBackBufferWidth = WIDTH;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            rand = new Random(System.DateTime.Now.Millisecond);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            projectiles = new List<ProjectileSprite>();
            particles = new List<Texture2D>();
            asteroids = new List<BouncingSprite>();
            spriteFont = Content.Load<SpriteFont>("font");

            //Load graphics
            earth = Content.Load<Texture2D>("am_rend_all");
            stars = Content.Load<Texture2D>("stars");
            explosion = Content.Load<Texture2D>("explosion");
            missile = Content.Load<Texture2D>("missile");
            particle = Content.Load<Texture2D>("particle");
            shuttle = Content.Load<Texture2D>("shuttle1");
            asteroid = Content.Load<Texture2D>("asteroid");

            //Load sound effects
            explode = Content.Load<SoundEffect>("Explode");
            gunshot = Content.Load<SoundEffect>("Gunshot");
            impact = Content.Load<SoundEffect>("impact");

            explosionEffect = new Explosion(explosion);
            particles.Add(particle);

            SHIP_POSITION = new Vector2(WIDTH / 2, HEIGHT / 2);
            SHIP_OFFSET = new Vector2(shuttle.Width / 2, shuttle.Height / 2);
            PROJECTILE_OFFSET = new Vector2(missile.Width / 2, 0);
            ASTEROID_OFFSET = new Vector2(asteroid.Width / 2, asteroid.Height / 2);

            ship = new UserControlledSprite(shuttle, SHIP_POSITION, SHIP_COLLISION_OFFSET, SHIP_SPEED, SHIP_OFFSET, SHIP_SCALE);
            
            for (int i = 0; i < NUM_ASTEROIDS; i++)
            {
                int paddingFromCenter = 85;
                bool upperLeft = rand.Next(2) == 1;
                int x = 0;
                int y = 0;
                if (upperLeft)
                {
                    x = rand.Next(0, WIDTH / 2 - paddingFromCenter);
                    y = rand.Next(0, HEIGHT / 2 - paddingFromCenter);
                }
                else
                {
                    x = rand.Next(WIDTH / 2 + paddingFromCenter, WIDTH);
                    y = rand.Next(HEIGHT / 2 + paddingFromCenter, HEIGHT);
                }
                Vector2 pos = new Vector2((float)x, (float)y);
                Vector2 dir = new Vector2((float)rand.NextDouble()*2 - 1, (float)rand.NextDouble()*2 - 1);
                dir.Normalize();
                float speed = (float)rand.NextDouble();
                float scale = rand.Next(75, 100) / 100.0f;
                float spinRate = rand.Next(15, 50) / 1000.0f;
                asteroids.Add(new BouncingSprite(asteroid, pos, ASTEROID_COLLISION_OFFSET,
                    speed, ASTEROID_OFFSET, scale, spinRate, dir, GraphicsDevice.Viewport.Bounds));
            }
        }

        protected override void Update(GameTime gameTime)
        {
            //Escape - quits the game
            if (Keyboard.GetState().GetPressedKeys().Contains<Keys>(Keys.Escape))
            {
                this.Exit();
            }

            explosionEffect.Update(gameTime);
            if (!lost)
            {
                //Space - fires a missile
                if (Keyboard.GetState().GetPressedKeys().Contains<Keys>(Keys.Space))
                {
                    fireMissile((int)gameTime.TotalGameTime.TotalMilliseconds);
                }

                ship.Update(gameTime, GraphicsDevice.Viewport.Bounds);
                int index = 0;
                List<int> ids = new List<int>();
                foreach (ProjectileSprite ps in projectiles)
                {
                    ps.Update(gameTime, GraphicsDevice.Viewport.Bounds);
                    if (ps.Position.X < -missile.Height * MISSILE_SCALE * 2 || ps.Position.X > WIDTH + missile.Height * MISSILE_SCALE * 2 ||
                        ps.Position.Y < -missile.Height * MISSILE_SCALE * 2 || ps.Position.Y > HEIGHT + missile.Height * MISSILE_SCALE * 2)
                    {
                        ids.Add(index);
                    }
                    index++;
                }
                for (int i = ids.Count - 1; i >= 0; i--)
                {
                    projectiles.RemoveAt(ids[i]);
                }

                //Check collision between ship/missile and asteroid
                int d = 0;
                foreach (BouncingSprite bs in asteroids)
                {
                    bs.Update(gameTime, GraphicsDevice.Viewport.Bounds);
                    if (ship.CollisionRect.Intersects(bs.CollisionRect))
                    {
                        lost = true;
                        explode.Play();
                        explosionEffect.AddExplosion(ship.Position, 75, 1500, 3500, gameTime);
                        break;
                    }
                    d++;
                }

                List<int> hitAsteroids = new List<int>();
                if(lost) hitAsteroids.Add(d);
                List<int> hitMissiles = new List<int>();
                for (int i = 0; i < asteroids.Count; i++)
                {
                    for (int j = 0; j < projectiles.Count; j++)
                    {
                        if (asteroids[i].CollisionRect.Intersects(projectiles[j].CollisionRect))
                        {
                            gunshot.Play();
                            hitAsteroids.Add(i);
                            hitMissiles.Add(j);
                            explosionEffect.AddExplosion(asteroids[i].Position, 20, 100, 1000, gameTime);
                        }
                    }
                }
                for (int i = hitAsteroids.Count - 1; i >= 0; i--)
                {
                    asteroids.RemoveAt(hitAsteroids[i]);
                }

                for (int i = hitMissiles.Count - 1; i >= 0; i--)
                {
                    projectiles.RemoveAt(hitMissiles[i]);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            
            Rectangle bounds = GraphicsDevice.Viewport.Bounds;
            Vector2 earthPosition = new Vector2(WIDTH - (0.85f)*earth.Width, HEIGHT - (1.05f)*earth.Height);
            //Draw background
            spriteBatch.Draw(stars, bounds, Color.White);
            spriteBatch.Draw(earth, earthPosition, earth.Bounds, Color.White);

            //Draw missiles
            foreach (ProjectileSprite ps in projectiles)
            {
                ps.Draw(gameTime, spriteBatch);
            }

            //Draw ship
            if(!lost)
            ship.Draw(gameTime, spriteBatch);

            foreach (BouncingSprite bs in asteroids)
            {
                bs.Draw(gameTime, spriteBatch);
            }

            if (lost) 
            spriteBatch.DrawString(spriteFont, "You Lose", new Vector2(50, 50), FONT_COLOR);

            explosionEffect.Draw(spriteBatch);

            if (DEBUG)
            {
                DrawBorder(ship.CollisionRect, 2, Color.Yellow);

                foreach (BouncingSprite bs in asteroids)
                {
                    DrawBorder(bs.CollisionRect, 2, Color.Red);
                }
                foreach (ProjectileSprite ps in projectiles)
                {
                    DrawBorder(ps.CollisionRect, 1, Color.Green);
                }
            }
            
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void fireMissile(int fireTime)
        {
            if (fireTime - missileFireTime > MISSILE_REFRESH)
            {
                impact.Play();
                missileFireTime = fireTime;
                projectiles.Add(new ProjectileSprite(missile, ship.Position, MISSILE_COLLISION_OFFSET,
                   MISSILE_SPEED, PROJECTILE_OFFSET, MISSILE_SCALE, ship.Rotation, particles));
            }
        }

        private void DrawBorder(Rectangle rectangleToDraw, int thicknessOfBorder, Color borderColor)
        {
            Texture2D pixel;
            pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData(new[] { Color.White });
            spriteBatch.Draw(pixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, thicknessOfBorder), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, thicknessOfBorder, rectangleToDraw.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle((rectangleToDraw.X + rectangleToDraw.Width - thicknessOfBorder),
                                            rectangleToDraw.Y,
                                            thicknessOfBorder,
                                            rectangleToDraw.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rectangleToDraw.X,
                                            rectangleToDraw.Y + rectangleToDraw.Height - thicknessOfBorder,
                                            rectangleToDraw.Width,
                                            thicknessOfBorder), borderColor);
        }
    }
}

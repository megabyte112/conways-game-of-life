using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
class Camera
{
    private Matrix transform;
    private Vector2 centre = new Vector2(3424, 1936);
    private Viewport viewport;
    private float zoom = 1.5f;
    private float scale;
    public Matrix Transform
    {
        get {return transform;}
    }
    public float X
    {
        get {return centre.X;}
        set {centre.X = value;}
    }
    public float Y
    {
        get {return centre.Y;}
        set {centre.Y = value;}
    }

    public float Zoom
    {
        get {return zoom;}
        set { zoom = value; if (zoom < 0.15f) zoom = 0.15f;}
    }

    public Camera(Viewport newViewport)
    {
        viewport = newViewport;
        scale = viewport.Width/1920f;
    }

    public void Update()
    {
        transform = Matrix.CreateTranslation(new Vector3(-centre.X, -centre.Y, 0))*
        Matrix.CreateScale(new Vector3(zoom, zoom, 1f))*
        Matrix.CreateScale(new Vector3(scale, scale, 1f))*
        Matrix.CreateTranslation(new Vector3(viewport.Width/2, viewport.Height/2, 0));
    }
    public void MoveVector(Vector2 deltamoved)
    {
        centre+=deltamoved;
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MineGame;

public static class Shapes {
    private static BasicEffect _effect;
    private static GraphicsDevice _gd;

    // =====================
    // Initialization
    // =====================
    public static void Initialize(GraphicsDevice gd) {
        _gd = gd;
        _effect = new BasicEffect(gd) {
            VertexColorEnabled = true,
            Projection = Matrix.CreateOrthographicOffCenter(0, gd.Viewport.Width, gd.Viewport.Height, 0, 0, 1)
        };
    }

    public static void UpdateProjection(int width, int height) {
        if (_effect != null) {
            _effect.Projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
        }
    }

    public static void Dispose() {
        _effect?.Dispose();
    }

    public static void SetTransform(Matrix transform) {
        if (_effect != null) {
            _effect.World = transform;
        }
    }

    public static void ResetTransform() {
        if (_effect != null) {
            _effect.World = Matrix.Identity;
        }
    }

    // =====================
    // Filled Polygon
    // =====================
    public static void DrawPolygon(GraphicsDevice gd, Vector2[] points, Color color, Matrix? transform = null) {
        if (points.Length < 3) return;

        VertexPositionColor[] vertices = new VertexPositionColor[(points.Length - 2) * 3];
        int vi = 0;

        for (int i = 1; i < points.Length - 1; i++) {
            vertices[vi++] = new VertexPositionColor(new Vector3(points[0], 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(points[i], 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(points[i + 1], 0), color);
        }

        Matrix oldWorld = _effect.World;
        _effect.World = transform ?? Matrix.Identity;

        foreach (var pass in _effect.CurrentTechnique.Passes) {
            pass.Apply();
            gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, points.Length - 2);
        }

        _effect.World = oldWorld;
    }

    // =====================
    // Polygon Outline
    // =====================
    public static void DrawPolygonOutline(GraphicsDevice gd, Vector2[] points, float thickness, Color color, Matrix? transform = null) {
        if (points.Length < 2) return;

        for (int i = 0; i < points.Length; i++) {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % points.Length];
            DrawLine(gd, a, b, thickness, color, transform);
        }
    }

    public static void DrawLine(GraphicsDevice gd, Vector2 a, Vector2 b, float thickness, Color color, Matrix? transform = null) {
        Vector2 dir = b - a;
        if (dir.LengthSquared() < 0.0001f) return;

        dir.Normalize();
        Vector2 normal = new Vector2(-dir.Y, dir.X) * (thickness * 0.5f);

        Vector2[] quad = [a + normal, a - normal, b - normal, b + normal];
        DrawPolygon(gd, quad, color, transform);
    }

    // =====================
    // Rectangle (filled)
    // =====================
    public static void DrawFillRect(GraphicsDevice gd, Vector2 topLeft, float width, float height, Color color, Matrix? transform = null) {
        Vector2[] rectPoints = [
        topLeft,
        topLeft + new Vector2(width, 0),
        topLeft + new Vector2(width, height),
        topLeft + new Vector2(0, height)
    ];
        DrawPolygon(gd, rectPoints, color, transform);
    }

    // =====================
    // Rectangle Outline
    // =====================
    public static void DrawRect(GraphicsDevice gd, Vector2 topLeft, float width, float height, float thickness, Color color, Matrix? transform = null) {
        Vector2 topRight = topLeft + new Vector2(width, 0);
        Vector2 bottomRight = topLeft + new Vector2(width, height);
        Vector2 bottomLeft = topLeft + new Vector2(0, height);

        DrawLine(gd, topLeft, topRight, thickness, color, transform);
        DrawLine(gd, topRight, bottomRight, thickness, color, transform);
        DrawLine(gd, bottomRight, bottomLeft, thickness, color, transform);
        DrawLine(gd, bottomLeft, topLeft, thickness, color, transform);
    }

    // =====================
    // Triangle (filled)
    // =====================
    public static void DrawTriangle(GraphicsDevice gd, Vector2 p1, Vector2 p2, Vector2 p3, Color color, Matrix? transform = null) {
        DrawPolygon(gd, [p1, p2, p3], color, transform);
    }

    // =====================
    // Triangle Outline
    // =====================
    public static void DrawTriangleOutline(GraphicsDevice gd, Vector2 p1, Vector2 p2, Vector2 p3, float thickness, Color color, Matrix? transform = null) {
        DrawLine(gd, p1, p2, thickness, color, transform);
        DrawLine(gd, p2, p3, thickness, color, transform);
        DrawLine(gd, p3, p1, thickness, color, transform);
    }

    // =====================
    // Circle
    // =====================
    public static void DrawCircle(GraphicsDevice gd, Vector2 center, float radius, int segments, Color color, Matrix? transform = null) {
        if (segments < 3) segments = 3;

        VertexPositionColor[] vertices = new VertexPositionColor[segments * 3];
        float angleStep = MathF.PI * 2 / segments;
        int vi = 0;

        for (int i = 0; i < segments; i++) {
            float angle1 = i * angleStep;
            float angle2 = (i + 1) * angleStep;

            vertices[vi++] = new VertexPositionColor(new Vector3(center, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(center.X + MathF.Cos(angle1) * radius, center.Y + MathF.Sin(angle1) * radius, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(center.X + MathF.Cos(angle2) * radius, center.Y + MathF.Sin(angle2) * radius, 0), color);
        }

        Matrix oldWorld = _effect.World;
        _effect.World = transform ?? Matrix.Identity;

        foreach (var pass in _effect.CurrentTechnique.Passes) {
            pass.Apply();
            gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, segments);
        }

        _effect.World = oldWorld;
    }

    // =====================
    // Circle Outline
    // =====================
    public static void DrawCircleOutline(GraphicsDevice gd, Vector2 center, float radius, float thickness, int segments, Color color, Matrix? transform = null) {
        if (segments < 3) segments = 3;

        Vector2[] points = new Vector2[segments];
        float angleStep = MathF.PI * 2 / segments;

        for (int i = 0; i < segments; i++) {
            float angle = i * angleStep;
            points[i] = new Vector2(center.X + MathF.Cos(angle) * radius, center.Y + MathF.Sin(angle) * radius);
        }

        DrawPolygonOutline(gd, points, thickness, color, transform);
    }

    public static void DrawRoundedRect(GraphicsDevice gd, Vector2 topLeft, float width, float height, float radius, float thickness, int segments, Color color, Matrix? transform = null) {
        if (segments < 3) segments = 3;
        float rx = Math.Min(radius, width / 2);
        float ry = Math.Min(radius, height / 2);

        Vector2[] corners = new Vector2[4 * segments];
        int idx = 0;

        for (int i = 0; i < segments; i++) {
            float angle = MathF.PI + MathF.PI / 2 * i / (segments - 1);
            corners[idx++] = topLeft + new Vector2(rx + MathF.Cos(angle) * rx, ry + MathF.Sin(angle) * ry);
        }

        for (int i = 0; i < segments; i++) {
            float angle = -MathF.PI / 2 + MathF.PI / 2 * i / (segments - 1);
            corners[idx++] = topLeft + new Vector2(width - rx + MathF.Cos(angle) * rx, ry + MathF.Sin(angle) * ry);
        }

        for (int i = 0; i < segments; i++) {
            float angle = 0 + MathF.PI / 2 * i / (segments - 1);
            corners[idx++] = topLeft + new Vector2(width - rx + MathF.Cos(angle) * rx, height - ry + MathF.Sin(angle) * ry);
        }

        for (int i = 0; i < segments; i++) {
            float angle = MathF.PI / 2 + MathF.PI / 2 * i / (segments - 1);
            corners[idx++] = topLeft + new Vector2(rx + MathF.Cos(angle) * rx, height - ry + MathF.Sin(angle) * ry);
        }

        DrawPolygonOutline(gd, corners, thickness, color, transform);
    }

    // =====================
    // Ellipse (filled)
    // =====================
    public static void DrawEllipse(GraphicsDevice gd, Vector2 center, float radiusX, float radiusY, int segments, Color color, Matrix? transform = null) {
        if (segments < 3) segments = 3;

        VertexPositionColor[] vertices = new VertexPositionColor[segments * 3];
        float angleStep = MathF.PI * 2 / segments;
        int vi = 0;

        for (int i = 0; i < segments; i++) {
            float angle1 = i * angleStep;
            float angle2 = (i + 1) * angleStep;

            vertices[vi++] = new VertexPositionColor(new Vector3(center, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(center.X + MathF.Cos(angle1) * radiusX, center.Y + MathF.Sin(angle1) * radiusY, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(center.X + MathF.Cos(angle2) * radiusX, center.Y + MathF.Sin(angle2) * radiusY, 0), color);
        }

        Matrix oldWorld = _effect.World;
        _effect.World = transform ?? Matrix.Identity;

        foreach (var pass in _effect.CurrentTechnique.Passes) {
            pass.Apply();
            gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, segments);
        }

        _effect.World = oldWorld;
    }

    // =====================
    // Ellipse Outline
    // =====================
    public static void DrawEllipseOutline(GraphicsDevice gd, Vector2 center, float radiusX, float radiusY, float thickness, int segments, Color color, Matrix? transform = null) {
        if (segments < 3) segments = 3;

        Vector2[] points = new Vector2[segments];
        float angleStep = MathF.PI * 2 / segments;

        for (int i = 0; i < segments; i++) {
            float angle = i * angleStep;
            points[i] = new Vector2(center.X + MathF.Cos(angle) * radiusX, center.Y + MathF.Sin(angle) * radiusY);
        }

        DrawPolygonOutline(gd, points, thickness, color, transform);
    }

    // =====================
    // Arc / Sector (filled)
    // =====================
    public static void DrawArc(GraphicsDevice gd, Vector2 center, float radius, float startAngle, float endAngle, int segments, Color color, Matrix? transform = null) {
        if (segments < 2) segments = 2;

        VertexPositionColor[] vertices = new VertexPositionColor[segments * 3];
        int vi = 0;
        float angleStep = (endAngle - startAngle) / segments;

        for (int i = 0; i < segments; i++) {
            float angle1 = startAngle + i * angleStep;
            float angle2 = startAngle + (i + 1) * angleStep;

            vertices[vi++] = new VertexPositionColor(new Vector3(center, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(center.X + MathF.Cos(angle1) * radius, center.Y + MathF.Sin(angle1) * radius, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(center.X + MathF.Cos(angle2) * radius, center.Y + MathF.Sin(angle2) * radius, 0), color);
        }

        Matrix oldWorld = _effect.World;
        _effect.World = transform ?? Matrix.Identity;

        foreach (var pass in _effect.CurrentTechnique.Passes) {
            pass.Apply();
            gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, segments);
        }

        _effect.World = oldWorld;
    }

    // =====================
    // Arc Outline (no fill)
    // =====================
    public static void DrawArcOutline(GraphicsDevice gd, Vector2 center, float radius, float startAngle, float endAngle, float thickness, int segments, Color color, Matrix? transform = null) {
        if (segments < 2) segments = 2;

        Vector2[] points = new Vector2[segments + 1];
        float angleStep = (endAngle - startAngle) / segments;

        for (int i = 0; i <= segments; i++) {
            float angle = startAngle + i * angleStep;
            points[i] = new Vector2(center.X + MathF.Cos(angle) * radius, center.Y + MathF.Sin(angle) * radius);
        }

        for (int i = 0; i < points.Length - 1; i++) {
            DrawLine(gd, points[i], points[i + 1], thickness, color, transform);
        }
    }

    // =====================
    // Quadratic Bezier
    // =====================
    public static void DrawQuadraticBezier(GraphicsDevice gd, Vector2 p0, Vector2 p1, Vector2 p2, float thickness, int segments, Color color, Matrix? transform = null) {
        if (segments < 2) segments = 2;

        Vector2[] points = new Vector2[segments + 1];

        for (int i = 0; i <= segments; i++) {
            float t = (float)i / segments;
            float u = 1 - t;
            points[i] = u * u * p0 + 2 * u * t * p1 + t * t * p2;
        }

        for (int i = 0; i < points.Length - 1; i++) {
            DrawLine(gd, points[i], points[i + 1], thickness, color, transform);
        }
    }

    // =====================
    // Cubic Bezier
    // =====================
    public static void DrawCubicBezier(GraphicsDevice gd, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float thickness, int segments, Color color, Matrix? transform = null) {
        if (segments < 2) segments = 2;

        Vector2[] points = new Vector2[segments + 1];

        for (int i = 0; i <= segments; i++) {
            float t = (float)i / segments;
            float u = 1 - t;
            points[i] = u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
        }

        for (int i = 0; i < points.Length - 1; i++) {
            DrawLine(gd, points[i], points[i + 1], thickness, color, transform);
        }
    }

    // =====================
    // Dashed Line
    // =====================
    public static void DrawDashedLine(GraphicsDevice gd, Vector2 a, Vector2 b, float thickness, float dashLength, float gapLength, Color color, Matrix? transform = null) {
        Vector2 dir = b - a;
        float totalLength = dir.Length();
        if (totalLength < 0.0001f) return;

        dir.Normalize();
        float patternLength = dashLength + gapLength;
        float currentLength = 0;

        while (currentLength < totalLength) {
            float segmentLength = Math.Min(dashLength, totalLength - currentLength);
            Vector2 start = a + dir * currentLength;
            Vector2 end = a + dir * (currentLength + segmentLength);
            DrawLine(gd, start, end, thickness, color, transform);
            currentLength += patternLength;
        }
    }

    // =====================
    // Dotted Line
    // =====================
    public static void DrawDottedLine(GraphicsDevice gd, Vector2 a, Vector2 b, float dotRadius, float spacing, Color color, Matrix? transform = null) {
        Vector2 dir = b - a;
        float totalLength = dir.Length();
        if (totalLength < 0.0001f) return;

        dir.Normalize();
        float currentLength = 0;
        while (currentLength <= totalLength) {
            Vector2 dotPos = a + dir * currentLength;
            DrawCircle(gd, dotPos, dotRadius, 8, color, transform);
            currentLength += spacing;
        }
    }

    // =====================
    // Star (filled)
    // =====================
    public static void DrawStar(GraphicsDevice gd, Vector2 center, float outerRadius, float innerRadius, int points, Color color, Matrix? transform = null) {
        if (points < 2) points = 2;

        VertexPositionColor[] vertices = new VertexPositionColor[points * 6];
        int vi = 0;
        float angleStep = MathF.PI * 2 / points;

        for (int i = 0; i < points; i++) {
            float outerAngle1 = i * angleStep - MathF.PI / 2;
            float outerAngle2 = (i + 1) % points * angleStep - MathF.PI / 2;
            float innerAngle = outerAngle1 + angleStep / 2;

            Vector2 outer1 = center + new Vector2(MathF.Cos(outerAngle1) * outerRadius, MathF.Sin(outerAngle1) * outerRadius);
            Vector2 outer2 = center + new Vector2(MathF.Cos(outerAngle2) * outerRadius, MathF.Sin(outerAngle2) * outerRadius);
            Vector2 inner = center + new Vector2(MathF.Cos(innerAngle) * innerRadius, MathF.Sin(innerAngle) * innerRadius);

            vertices[vi++] = new VertexPositionColor(new Vector3(center, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(outer1, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(inner, 0), color);

            vertices[vi++] = new VertexPositionColor(new Vector3(center, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(inner, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(outer2, 0), color);
        }

        Matrix oldWorld = _effect.World;
        _effect.World = transform ?? Matrix.Identity;

        foreach (var pass in _effect.CurrentTechnique.Passes) {
            pass.Apply();
            gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, points * 2);
        }

        _effect.World = oldWorld;
    }

    // =====================
    // Star Outline
    // =====================
    public static void DrawStarOutline(GraphicsDevice gd, Vector2 center, float outerRadius, float innerRadius, int points, float thickness, Color color, Matrix? transform = null) {
        if (points < 3) points = 3;

        Vector2[] starPoints = new Vector2[points * 2];
        float angleStep = MathF.PI * 2 / points;

        for (int i = 0; i < points; i++) {
            float outerAngle = i * angleStep - MathF.PI / 2;
            float innerAngle = outerAngle + angleStep / 2;
            starPoints[i * 2] = center + new Vector2(MathF.Cos(outerAngle) * outerRadius, MathF.Sin(outerAngle) * outerRadius);
            starPoints[i * 2 + 1] = center + new Vector2(MathF.Cos(innerAngle) * innerRadius, MathF.Sin(innerAngle) * innerRadius);
        }

        DrawPolygonOutline(gd, starPoints, thickness, color, transform);
    }

    // =====================
    // Arrow
    // =====================
    public static void DrawArrow(GraphicsDevice gd, Vector2 start, Vector2 end, float thickness, float arrowHeadSize, Color color, Matrix? transform = null) {
        DrawLine(gd, start, end, thickness, color, transform);

        Vector2 dir = end - start;
        if (dir.LengthSquared() < 0.0001f) return;

        dir.Normalize();
        Vector2 perp = new(-dir.Y, dir.X);
        Vector2 arrowPoint1 = end - dir * arrowHeadSize + perp * (arrowHeadSize * 0.5f);
        Vector2 arrowPoint2 = end - dir * arrowHeadSize - perp * (arrowHeadSize * 0.5f);

        DrawTriangle(gd, end, arrowPoint1, arrowPoint2, color, transform);
    }

    // =====================
    // Rounded Rectangle (filled)
    // =====================
    public static void DrawRoundedRectFill(GraphicsDevice gd, Vector2 topLeft, float width, float height, float radius, int segments, Color color, Matrix? transform = null) {
        if (segments < 3) segments = 3;
        float rx = Math.Min(radius, width / 2);
        float ry = Math.Min(radius, height / 2);

        Vector2[] points = new Vector2[4 * segments];
        int idx = 0;

        for (int i = 0; i < segments; i++) {
            float angle = MathF.PI + MathF.PI / 2 * i / (segments - 1);
            points[idx++] = topLeft + new Vector2(rx + MathF.Cos(angle) * rx, ry + MathF.Sin(angle) * ry);
        }

        for (int i = 0; i < segments; i++) {
            float angle = -MathF.PI / 2 + MathF.PI / 2 * i / (segments - 1);
            points[idx++] = topLeft + new Vector2(width - rx + MathF.Cos(angle) * rx, ry + MathF.Sin(angle) * ry);
        }

        for (int i = 0; i < segments; i++) {
            float angle = 0 + MathF.PI / 2 * i / (segments - 1);
            points[idx++] = topLeft + new Vector2(width - rx + MathF.Cos(angle) * rx, height - ry + MathF.Sin(angle) * ry);
        }

        for (int i = 0; i < segments; i++) {
            float angle = MathF.PI / 2 + MathF.PI / 2 * i / (segments - 1);
            points[idx++] = topLeft + new Vector2(rx + MathF.Cos(angle) * rx, height - ry + MathF.Sin(angle) * ry);
        }

        DrawPolygon(gd, points, color, transform);
    }

    // =====================
    // Pie / Sector (filled)
    // =====================
    public static void DrawPie(GraphicsDevice gd, Vector2 center, float radius, float startAngle, float endAngle, int segments, Color color, Matrix? transform = null) {
        if (segments < 2) segments = 2;

        VertexPositionColor[] vertices = new VertexPositionColor[segments * 3];
        int vi = 0;
        float angleStep = (endAngle - startAngle) / segments;

        for (int i = 0; i < segments; i++) {
            float angle1 = startAngle + i * angleStep;
            float angle2 = startAngle + (i + 1) * angleStep;

            vertices[vi++] = new VertexPositionColor(new Vector3(center, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(center.X + MathF.Cos(angle1) * radius, center.Y + MathF.Sin(angle1) * radius, 0), color);
            vertices[vi++] = new VertexPositionColor(new Vector3(center.X + MathF.Cos(angle2) * radius, center.Y + MathF.Sin(angle2) * radius, 0), color);
        }

        Matrix oldWorld = _effect.World;
        _effect.World = transform ?? Matrix.Identity;

        foreach (var pass in _effect.CurrentTechnique.Passes) {
            pass.Apply();
            gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, segments);
        }

        _effect.World = oldWorld;
    }

    // =====================
    // Filled Quadratic Bezier (polygon approximation)
    // =====================
    public static void DrawQuadraticBezierFill(GraphicsDevice gd, Vector2 p0, Vector2 p1, Vector2 p2, int segments, Color color, Matrix? transform = null) {
        if (segments < 2) segments = 2;

        Vector2[] points = new Vector2[segments + 2];
        points[0] = p0;

        for (int i = 0; i <= segments; i++) {
            float t = (float)i / segments;
            float u = 1 - t;
            points[i + 1] = u * u * p0 + 2 * u * t * p1 + t * t * p2;
        }

        DrawPolygon(gd, points, color, transform);
    }

    // =====================
    // Filled Cubic Bezier (polygon approximation)
    // =====================
    public static void DrawCubicBezierFill(GraphicsDevice gd, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int segments, Color color, Matrix? transform = null) {
        if (segments < 2) segments = 2;

        Vector2[] points = new Vector2[segments + 2];
        points[0] = p0;

        for (int i = 0; i <= segments; i++) {
            float t = (float)i / segments;
            float u = 1 - t;
            points[i + 1] = u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
        }

        DrawPolygon(gd, points, color, transform);
    }
}
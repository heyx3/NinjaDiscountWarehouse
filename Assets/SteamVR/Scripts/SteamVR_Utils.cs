//========= Copyright 2014, Valve Corporation, All rights reserved. ===========
//
// Purpose: Utilities for working with SteamVR
//
//=============================================================================

using UnityEngine;
using System.Collections;

public static class SteamVR_Utils
{
	public class Event
	{
		public delegate void Handler(params object[] args);

		public static void Listen(string message, Handler action)
		{
			var actions = listeners[message] as Handler;
			if (actions != null)
			{
				listeners[message] = actions + action;
			}
			else
			{
				listeners[message] = action;
			}
		}

		public static void Remove(string message, Handler action)
		{
			var actions = listeners[message] as Handler;
			if (actions != null)
			{
				listeners[message] = actions - action;
			}
		}

		public static void Send(string message, params object[] args)
		{
			var actions = listeners[message] as Handler;
			if (actions != null)
			{
				actions(args);
			}
		}

		private static Hashtable listeners = new Hashtable();
	}

	// this version does not clamp [0..1]
	public static Quaternion Slerp(Quaternion A, Quaternion B, float t)
	{
		var cosom = Mathf.Clamp(A.x * B.x + A.y * B.y + A.z * B.z + A.w * B.w, -1.0f, 1.0f);
		if (cosom < 0.0f)
		{
			B = new Quaternion(-B.x, -B.y, -B.z, -B.w);
			cosom = -cosom;
		}

		float sclp, sclq;
		if ((1.0f - cosom) > 0.0001f)
		{
			var omega = Mathf.Acos(cosom);
			var sinom = Mathf.Sin(omega);
			sclp = Mathf.Sin((1.0f - t) * omega) / sinom;
			sclq = Mathf.Sin(t * omega) / sinom;
		}
		else
		{
			// "from" and "to" very close, so do linear interp
			sclp = 1.0f - t;
			sclq = t;
		}

		return new Quaternion(
			sclp * A.x + sclq * B.x,
			sclp * A.y + sclq * B.y,
			sclp * A.z + sclq * B.z,
			sclp * A.w + sclq * B.w);
	}

	public static Vector3 Lerp(Vector3 A, Vector3 B, float t)
	{
		return new Vector3(
			Lerp(A.x, B.x, t),
			Lerp(A.y, B.y, t),
			Lerp(A.z, B.z, t));
	}

	public static float Lerp(float A, float B, float t)
	{
		return A + (B - A) * t;
	}

	public static double Lerp(double A, double B, double t)
	{
		return A + (B - A) * t;
	}

	public static float InverseLerp(Vector3 A, Vector3 B, Vector3 result)
	{
		return Vector3.Dot(result - A, B - A);
	}

	public static float InverseLerp(float A, float B, float result)
	{
		return (result - A) / (B - A);
	}

	public static double InverseLerp(double A, double B, double result)
	{
		return (result - A) / (B - A);
	}

	public static Quaternion GetRotation(this Matrix4x4 matrix)
	{
		var tr = 1f + matrix.m00 + matrix.m11 + matrix.m22;
		if (tr > 1e-8f)
		{
			var s = Mathf.Sqrt(tr) * 2f;
			var invS = 1f / s;
			return new Quaternion(
				(matrix.m21 - matrix.m12) * invS,
				(matrix.m02 - matrix.m20) * invS,
				(matrix.m10 - matrix.m01) * invS,
				0.25f * s);
		}
		return Quaternion.identity;
	}

	public static Vector3 GetPosition(this Matrix4x4 matrix)
	{
		var x = matrix.m03;
		var y = matrix.m13;
		var z = matrix.m23;

		return new Vector3(x, y, z);
	}

	public static Vector3 GetScale(this Matrix4x4 m)
	{
		var x = Mathf.Sqrt(m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02);
		var y = Mathf.Sqrt(m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12);
		var z = Mathf.Sqrt(m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22);

		return new Vector3(x, y, z);
	}
	
	[System.Serializable]
	public struct RigidTransform
	{
		public Vector3 pos;
		public Quaternion rot;

		public static RigidTransform identity
		{
			get { return new RigidTransform(Vector3.zero, Quaternion.identity); }
		}

		public static RigidTransform FromLocal(Transform t)
		{
			return new RigidTransform(t.localPosition, t.localRotation);
		}

		public RigidTransform(Vector3 pos, Quaternion rot)
		{
			this.pos = pos;
			this.rot = rot;
		}

		public RigidTransform(Transform t)
		{
			this.pos = t.position;
			this.rot = t.rotation;
		}

		public RigidTransform(Transform from, Transform to)
		{
			var inv = Quaternion.Inverse(from.rotation);
			rot = inv * to.rotation;
			pos = inv * (to.position - from.position);
		}

		public RigidTransform(SteamVR.HmdMatrix34_t pose)
		{
			var m = Matrix4x4.identity;

			m[0, 0] = pose.m[0 * 4 + 0];
			m[1, 0] = pose.m[1 * 4 + 0];
			m[2, 0] = -pose.m[2 * 4 + 0];

			m[0, 1] = pose.m[0 * 4 + 1];
			m[1, 1] = pose.m[1 * 4 + 1];
			m[2, 1] = -pose.m[2 * 4 + 1];

			m[0, 2] = -pose.m[0 * 4 + 2];
			m[1, 2] = -pose.m[1 * 4 + 2];
			m[2, 2] = pose.m[2 * 4 + 2];

			m[0, 3] = pose.m[0 * 4 + 3];
			m[1, 3] = pose.m[1 * 4 + 3];
			m[2, 3] = -pose.m[2 * 4 + 3];

			this.pos = m.GetPosition();
			this.rot = m.GetRotation();
		}

		public RigidTransform(SteamVR.HmdMatrix44_t pose)
		{
			var m = Matrix4x4.identity;

			m[0, 0] = pose.m[0 * 4 + 0];
			m[1, 0] = pose.m[1 * 4 + 0];
			m[2, 0] = -pose.m[2 * 4 + 0];
			m[3, 0] = pose.m[3 * 4 + 0];

			m[0, 1] = pose.m[0 * 4 + 1];
			m[1, 1] = pose.m[1 * 4 + 1];
			m[2, 1] = -pose.m[2 * 4 + 1];
			m[3, 1] = pose.m[3 * 4 + 1];

			m[0, 2] = -pose.m[0 * 4 + 2];
			m[1, 2] = -pose.m[1 * 4 + 2];
			m[2, 2] = pose.m[2 * 4 + 2];
			m[3, 2] = -pose.m[3 * 4 + 2];

			m[0, 3] = pose.m[0 * 4 + 3];
			m[1, 3] = pose.m[1 * 4 + 3];
			m[2, 3] = -pose.m[2 * 4 + 3];
			m[3, 3] = pose.m[3 * 4 + 3];

			this.pos = m.GetPosition();
			this.rot = m.GetRotation();
		}

		public override bool Equals(object o)
		{
			if (o is RigidTransform)
			{
				RigidTransform t = (RigidTransform)o;
				return pos == t.pos && rot == t.rot;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return pos.GetHashCode() ^ rot.GetHashCode();
		}

		public static bool operator ==(RigidTransform a, RigidTransform b)
		{
			return a.pos == b.pos && a.rot == b.rot;
		}

		public static bool operator !=(RigidTransform a, RigidTransform b)
		{
			return a.pos != b.pos || a.rot != b.rot;
		}

		public static RigidTransform operator *(RigidTransform a, RigidTransform b)
		{
			return new RigidTransform
			{
				rot = a.rot * b.rot,
				pos = a.pos + a.rot * b.pos
			};
		}

		public void Inverse()
		{
			rot = Quaternion.Inverse(rot);
			pos = -(rot * pos);
		}

		public RigidTransform GetInverse()
		{
			var t = new RigidTransform(pos, rot);
			t.Inverse();
			return t;
		}

		public void Multiply(RigidTransform a, RigidTransform b)
		{
			rot = a.rot * b.rot;
			pos = a.pos + a.rot * b.pos;
		}

		public Vector3 InverseTransformPoint(Vector3 point)
		{
			return Quaternion.Inverse(rot) * (point - pos);
		}

		public Vector3 TransformPoint(Vector3 point)
		{
			return pos + (rot * point);
		}

		public static Vector3 operator *(RigidTransform t, Vector3 v)
		{
			return t.TransformPoint(v);
		}

		public static RigidTransform Interpolate(RigidTransform a, RigidTransform b, float t)
		{
			return new RigidTransform(Vector3.Lerp(a.pos, b.pos, t), Quaternion.Slerp(a.rot, b.rot, t));
		}

		public void Interpolate(RigidTransform to, float t)
		{
			pos = SteamVR_Utils.Lerp(pos, to.pos, t);
			rot = SteamVR_Utils.Slerp(rot, to.rot, t);
		}
	}

	// Builds a tessellated grid of quads across the provided bounds with three sets of UVs used to account for
	// lens distortion and chromatic aberration.
	public static Mesh CreateDistortMesh(SteamVR.IHmd hmd, SteamVR.Hmd_Eye eye, int resU, int resV, Rect bounds)
	{
		int numVerts = resU * resV;
		var vertices = new Vector3[numVerts];
		var redUVs = new Vector3[numVerts];
		var greenUVs = new Vector2[numVerts];
		var blueUVs = new Vector2[numVerts];

		int iTri = 0;
		int resUm1 = resU - 1;
		int resVm1 = resV - 1;
		var triangles = new int[2 * 3 * resUm1 * resVm1];

		for (int iV = 0; iV < resV; iV++)
		{
			float v = (float)iV / resVm1;
			float y = SteamVR_Utils.Lerp(bounds.yMax, bounds.yMin, v); // ComputeDistortion expects 0,0 in upper left
			for (int iU = 0; iU < resU; iU++)
			{
				float u = (float)iU / resUm1;
				float x = SteamVR_Utils.Lerp(bounds.xMin, bounds.xMax, u);
				var coords = hmd.ComputeDistortion(eye, u, v);

				int idx = iV * resU + iU;
				vertices[idx] = new Vector3(x, y, 0.1f);

				redUVs[idx] = new Vector3(coords.rfRed[0], 1.0f - coords.rfRed[1], 0);
				greenUVs[idx] = new Vector2(coords.rfGreen[0], 1.0f - coords.rfGreen[1]);
				blueUVs[idx] = new Vector2(coords.rfBlue[0], 1.0f - coords.rfBlue[1]);

				if (iV > 0 && iU > 0)
				{
					int a = (iV - 1) * resU + (iU - 1);
					int b = (iV - 1) * resU + iU;
					int c = iV * resU + (iU - 1);
					int d = iV * resU + iU;

					triangles[iTri++] = a;
					triangles[iTri++] = b;
					triangles[iTri++] = d;

					triangles[iTri++] = a;
					triangles[iTri++] = d;
					triangles[iTri++] = c;
				}
			}
		}

		var mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.normals = redUVs; // stuff data in normals since only two uv sets exposed
		mesh.uv = greenUVs;
		mesh.uv2 = blueUVs;
		mesh.triangles = triangles;
		return mesh;
	}

	// Draw a unit cube centered on the camera with the provided cube map.
	public static void DrawSkybox(Material material, Quaternion rotation, Matrix4x4 projection, bool clearDepth = true)
	{
		GL.Clear(clearDepth, GL.wireframe, Color.clear);
		if (material == null)
			return;

		var view = Matrix4x4.TRS(Vector3.zero, Quaternion.Inverse(rotation), Vector3.one);

		// Convert from Unity coords (flip z-axis)
		view[0, 2] = -view[0, 2];
		view[1, 2] = -view[1, 2];
		view[2, 0] = -view[2, 0];
		view[2, 1] = -view[2, 1];
		view[2, 3] = -view[2, 3];
		view[3, 2] = -view[3, 2];

		GL.PushMatrix();

		if (material.passCount == 1)
		{
			GL.LoadOrtho();
			GL.Begin(GL.QUADS);
			material.SetPass(0);

			var m = (projection * view).inverse;
			var verts = new Vector4[] {
				new Vector4(-1, 1, 1, 1),
				new Vector4(1, 1, 1, 1),
				new Vector4(1, -1, 1, 1),
				new Vector4(-1, -1, 1, 1)
			};
			for (int i = 0; i < verts.Length; i++)
			{
				var v = verts[i];
				var pos = m * v;
				var tex = new Vector3(pos.x, pos.y, pos.z) / pos.w;
				GL.TexCoord3(tex.x, tex.y, tex.z);
				GL.Vertex3((v.x + 1.0f) * 0.5f, (v.y + 1.0f) * 0.5f, 0);
			}

			GL.End();
		}
		else if (material.passCount == 6)
		{
			GL.LoadIdentity();
			GL.MultMatrix(view);
			GL.LoadProjectionMatrix(projection);

			bool d3d9 = SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 9");
			float u0 = 0, v0 = 0, u1 = 1, v1 = 1;

			var tex = material.GetTexture("_FrontTex");
			if (tex != null)
			{
				if (d3d9)
				{
					u0 = 0.5f / tex.width;
					u1 = u0 + 1.0f;

					v0 = 0.5f / tex.height;
					v1 = v0 + 1.0f;
				}

				GL.Begin(GL.QUADS);
				material.SetPass(0);
				GL.TexCoord2(u1, v0); GL.Vertex3(1, -1, -1);
				GL.TexCoord2(u1, v1); GL.Vertex3(1, 1, -1);
				GL.TexCoord2(u0, v1); GL.Vertex3(-1, 1, -1);
				GL.TexCoord2(u0, v0); GL.Vertex3(-1, -1, -1);
				GL.End();
			}

			tex = material.GetTexture("_BackTex");
			if (tex != null)
			{
				if (d3d9)
				{
					u0 = 0.5f / tex.width;
					u1 = u0 + 1.0f;

					v0 = 0.5f / tex.height;
					v1 = v0 + 1.0f;
				}

				GL.Begin(GL.QUADS);
				material.SetPass(1);
				GL.TexCoord2(u1, v0); GL.Vertex3(-1, -1, 1);
				GL.TexCoord2(u1, v1); GL.Vertex3(-1, 1, 1);
				GL.TexCoord2(u0, v1); GL.Vertex3(1, 1, 1);
				GL.TexCoord2(u0, v0); GL.Vertex3(1, -1, 1);
				GL.End();
			}

			tex = material.GetTexture("_LeftTex");
			if (tex != null)
			{
				if (d3d9)
				{
					u0 = 0.5f / tex.width;
					u1 = u0 + 1.0f;

					v0 = 0.5f / tex.height;
					v1 = v0 + 1.0f;
				}

				GL.Begin(GL.QUADS);
				material.SetPass(2);
				GL.TexCoord2(u1, v0); GL.Vertex3(1, -1, 1);
				GL.TexCoord2(u1, v1); GL.Vertex3(1, 1, 1);
				GL.TexCoord2(u0, v1); GL.Vertex3(1, 1, -1);
				GL.TexCoord2(u0, v0); GL.Vertex3(1, -1, -1);
				GL.End();
			}

			tex = material.GetTexture("_RightTex");
			if (tex != null)
			{
				if (d3d9)
				{
					u0 = 0.5f / tex.width;
					u1 = u0 + 1.0f;

					v0 = 0.5f / tex.height;
					v1 = v0 + 1.0f;
				}

				GL.Begin(GL.QUADS);
				material.SetPass(3);
				GL.TexCoord2(u1, v0); GL.Vertex3(-1, -1, -1);
				GL.TexCoord2(u1, v1); GL.Vertex3(-1, 1, -1);
				GL.TexCoord2(u0, v1); GL.Vertex3(-1, 1, 1);
				GL.TexCoord2(u0, v0); GL.Vertex3(-1, -1, 1);
				GL.End();
			}

			tex = material.GetTexture("_UpTex");
			if (tex != null)
			{
				if (d3d9)
				{
					u0 = 0.5f / tex.width;
					u1 = u0 + 1.0f;

					v0 = 0.5f / tex.height;
					v1 = v0 + 1.0f;
				}

				GL.Begin(GL.QUADS);
				material.SetPass(4);
				GL.TexCoord2(u1, v0); GL.Vertex3(1, 1, -1);
				GL.TexCoord2(u1, v1); GL.Vertex3(1, 1, 1);
				GL.TexCoord2(u0, v1); GL.Vertex3(-1, 1, 1);
				GL.TexCoord2(u0, v0); GL.Vertex3(-1, 1, -1);
				GL.End();
			}

			tex = material.GetTexture("_DownTex");
			if (tex != null)
			{
				if (d3d9)
				{
					u0 = 0.5f / tex.width;
					u1 = u0 + 1.0f;

					v0 = 0.5f / tex.height;
					v1 = v0 + 1.0f;
				}

				GL.Begin(GL.QUADS);
				material.SetPass(5);
				GL.TexCoord2(u1, v0); GL.Vertex3(1, -1, 1);
				GL.TexCoord2(u1, v1); GL.Vertex3(1, -1, -1);
				GL.TexCoord2(u0, v1); GL.Vertex3(-1, -1, -1);
				GL.TexCoord2(u0, v0); GL.Vertex3(-1, -1, 1);
				GL.End();
			}
		}

		GL.PopMatrix();
	}
}


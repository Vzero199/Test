using System;

public static class SoundLoopBus
{
	public static Action<Entities> OnLoopSoundPlay;

	public static void RaiseLoopSound(Entities entity)
	{
		OnLoopSoundPlay?.Invoke(entity);
	}
}

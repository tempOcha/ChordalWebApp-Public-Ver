using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

public static class MidiExportHelper
{
    // MIDI constants
    private const byte MIDI_HEADER_CHUNK = 0x4D; // 'M' in MThd
    private const byte MIDI_TRACK_CHUNK = 0x4D;  // 'M' in MTrk
    private const int DEFAULT_TEMPO = 120; // BPM
    private const int TICKS_PER_QUARTER = 480; // Standard MIDI resolution
    private const byte NOTE_ON = 0x90;
    private const byte NOTE_OFF = 0x80;
    private const byte DEFAULT_VELOCITY = 64; // Medium velocity

    /// <summary>
    /// Converts a chord progression to a MIDI file byte array
    /// </summary>
    /// <param name="progressionTitle">Title of the progression</param>
    /// <param name="tempo">Tempo in BPM (defaults to 120)</param>
    /// <param name="chordEvents">List of chord events with timing and note information</param>
    /// <returns>Byte array containing complete MIDI file data</returns>
    public static byte[] ConvertProgressionToMidi(
        string progressionTitle,
        int? tempo,
        List<ChordEventData> chordEvents)
    {
        if (chordEvents == null || chordEvents.Count == 0)
        {
            throw new ArgumentException("Cannot export progression with no chord events");
        }

        int actualTempo = tempo ?? DEFAULT_TEMPO;

        using (MemoryStream midiStream = new MemoryStream())
        {
            // Write MIDI header
            WriteMidiHeader(midiStream);

            // Write track with chord events
            WriteTrackData(midiStream, progressionTitle, actualTempo, chordEvents);

            return midiStream.ToArray();
        }
    }

    /// <summary>
    /// Writes the MIDI file header chunk
    /// Format: MThd + chunk length + format type + number of tracks + ticks per quarter note
    /// </summary>
    private static void WriteMidiHeader(MemoryStream stream)
    {
        // "MThd" chunk identifier
        stream.Write(new byte[] { 0x4D, 0x54, 0x68, 0x64 }, 0, 4);

        // Header length (always 6 bytes)
        stream.Write(new byte[] { 0x00, 0x00, 0x00, 0x06 }, 0, 4);

        // Format type 0 (single track)
        stream.Write(new byte[] { 0x00, 0x00 }, 0, 2);

        // Number of tracks (1)
        stream.Write(new byte[] { 0x00, 0x01 }, 0, 2);

        // Ticks per quarter note
        byte[] ticksBytes = BitConverter.GetBytes((short)TICKS_PER_QUARTER);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(ticksBytes);
        stream.Write(ticksBytes, 0, 2);
    }

    /// <summary>
    /// Writes the track data containing all chord events
    /// </summary>
    private static void WriteTrackData(
        MemoryStream stream,
        string title,
        int tempo,
        List<ChordEventData> chordEvents)
    {
        using (MemoryStream trackData = new MemoryStream())
        {
            // Track name meta event
            WriteTrackName(trackData, title);

            // Tempo meta event
            WriteTempoEvent(trackData, tempo);

            // Time signature (4/4)
            WriteTimeSignature(trackData);

            // Convert chord events to MIDI note events
            WriteChordEvents(trackData, chordEvents);

            // End of track meta event
            WriteEndOfTrack(trackData);

            // Write MTrk header
            stream.Write(new byte[] { 0x4D, 0x54, 0x72, 0x6B }, 0, 4); // "MTrk"

            // Write track length
            byte[] trackLength = BitConverter.GetBytes((int)trackData.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(trackLength);
            stream.Write(trackLength, 0, 4);

            // Write track data
            trackData.Position = 0;
            trackData.CopyTo(stream);
        }
    }

    /// <summary>
    /// Writes track name meta event
    /// </summary>
    private static void WriteTrackName(MemoryStream stream, string title)
    {
        stream.WriteByte(0x00); // Delta time
        stream.WriteByte(0xFF); // Meta event
        stream.WriteByte(0x03); // Track name

        byte[] titleBytes = System.Text.Encoding.ASCII.GetBytes(title ?? "Chord Progression");
        WriteVariableLength(stream, titleBytes.Length);
        stream.Write(titleBytes, 0, titleBytes.Length);
    }

    /// <summary>
    /// Writes tempo meta event
    /// </summary>
    private static void WriteTempoEvent(MemoryStream stream, int bpm)
    {
        stream.WriteByte(0x00); // Delta time
        stream.WriteByte(0xFF); // Meta event
        stream.WriteByte(0x51); // Set tempo
        stream.WriteByte(0x03); // Length

        // Calculate microseconds per quarter note
        int microsecondsPerQuarter = 60000000 / bpm;
        byte[] tempoBytes = BitConverter.GetBytes(microsecondsPerQuarter);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(tempoBytes);

        // Write only the 3 most significant bytes
        stream.Write(tempoBytes, 1, 3);
    }

    /// <summary>
    /// Writes time signature meta event (4/4)
    /// </summary>
    private static void WriteTimeSignature(MemoryStream stream)
    {
        stream.WriteByte(0x00); // Delta time
        stream.WriteByte(0xFF); // Meta event
        stream.WriteByte(0x58); // Time signature
        stream.WriteByte(0x04); // Length
        stream.WriteByte(0x04); // Numerator (4)
        stream.WriteByte(0x02); // Denominator (2^2 = 4)
        stream.WriteByte(0x18); // MIDI clocks per metronome tick
        stream.WriteByte(0x08); // 32nd notes per quarter note
    }

    /// <summary>
    /// Writes all chord events as MIDI note on/off events
    /// </summary>
    private static void WriteChordEvents(MemoryStream stream, List<ChordEventData> chordEvents)
    {
        double currentTime = 0.0;

        foreach (var chordEvent in chordEvents.OrderBy(e => e.StartTime))
        {
            // Calculate delta time from last event
            double deltaSeconds = chordEvent.StartTime - currentTime;
            int deltaTicks = (int)(deltaSeconds * TICKS_PER_QUARTER * 2); // Assuming quarter note = 0.5 seconds at 120 BPM

            // Parse notes from CSV
            List<int> notes = ParseNotesFromCsv(chordEvent.NotesCSV);

            if (notes.Count > 0)
            {
                // Write note on events
                bool firstNote = true;
                foreach (int note in notes)
                {
                    if (note >= 0 && note <= 127)
                    {
                        WriteVariableLength(stream, firstNote ? deltaTicks : 0);
                        stream.WriteByte(NOTE_ON);
                        stream.WriteByte((byte)note);
                        stream.WriteByte(DEFAULT_VELOCITY);
                        firstNote = false;
                    }
                }

                // Calculate duration in ticks
                int durationTicks = (int)(chordEvent.Duration * TICKS_PER_QUARTER * 2);

                // Write note off events
                firstNote = true;
                foreach (int note in notes)
                {
                    if (note >= 0 && note <= 127)
                    {
                        WriteVariableLength(stream, firstNote ? durationTicks : 0);
                        stream.WriteByte(NOTE_OFF);
                        stream.WriteByte((byte)note);
                        stream.WriteByte(0x00);
                        firstNote = false;
                    }
                }

                currentTime = chordEvent.StartTime + chordEvent.Duration;
            }
        }
    }

    /// <summary>
    /// Writes end of track meta event
    /// </summary>
    private static void WriteEndOfTrack(MemoryStream stream)
    {
        stream.WriteByte(0x00); // Delta time
        stream.WriteByte(0xFF); // Meta event
        stream.WriteByte(0x2F); // End of track
        stream.WriteByte(0x00); // Length
    }

    /// <summary>
    /// Writes a variable length quantity (MIDI standard)
    /// </summary>
    private static void WriteVariableLength(MemoryStream stream, int value)
    {
        if (value < 0) value = 0;

        List<byte> bytes = new List<byte>();

        bytes.Add((byte)(value & 0x7F));
        value >>= 7;

        while (value > 0)
        {
            bytes.Insert(0, (byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }

        stream.Write(bytes.ToArray(), 0, bytes.Count);
    }

    /// <summary>
    /// Parses MIDI note numbers from CSV string
    /// </summary>
    private static List<int> ParseNotesFromCsv(string notesCSV)
    {
        List<int> notes = new List<int>();

        if (string.IsNullOrEmpty(notesCSV))
            return notes;

        string[] noteStrings = notesCSV.Split(',');
        foreach (string noteStr in noteStrings)
        {
            if (int.TryParse(noteStr.Trim(), out int note))
            {
                notes.Add(note);
            }
        }

        return notes;
    }
}

/// <summary>
/// Data structure for chord event information
/// </summary>
public class ChordEventData
{
    public double StartTime { get; set; }
    public double Duration { get; set; }
    public string ChordName { get; set; }
    public string NotesCSV { get; set; }
    public int SequenceOrder { get; set; }
}
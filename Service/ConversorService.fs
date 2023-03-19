﻿module ConversorService

open System.IO
open FFMpegCore
open FFMpegCore.Enums
open CustomSettings

let getConversionOptions (options: FFMpegArgumentOptions) (codec: string) =
    match codec with
    | "mp3" ->  options.WithAudioBitrate(AudioQuality.Good)
                        .WithAudioCodec(AudioCodec.LibMp3Lame) |> ignore
    | "ac3" ->  options.WithAudioBitrate(AudioQuality.Good)
                        .WithAudioCodec(AudioCodec.Ac3) |> ignore
    | "ogg" ->  options.WithAudioBitrate(AudioQuality.Good)
                        .WithAudioCodec(AudioCodec.LibVorbis) |> ignore
    | _ -> options.WithAudioBitrate(AudioQuality.Normal)
                    .WithAudioCodec(AudioCodec.LibMp3Lame) |> ignore

let processFile (codec: string) (outputfolder: string) (file:string) =
    async {
        let workingfolder = Path.Join([|outputfolder; Path.GetDirectoryName(file).Split('\\') |> List.ofArray |> List.rev |> List.head|])

        match File.Exists workingfolder with
        | false -> Directory.CreateDirectory workingfolder |> ignore
        | true -> ()

        printfn "Processing file '%s' ..." file
        let outputFile = Path.Join([|workingfolder; (Path.GetFileName(file).Replace("flac", codec)) |]) 

        let processor = FFMpegArguments
                            .FromFileInput(file)
                            .OutputToFile(outputFile, true, fun options -> getConversionOptions options codec)

        processor.ProcessSynchronously() |> ignore
        printfn "Processed! File saved to '%s' %s" outputFile System.Environment.NewLine
    }

let ProcessFolder (outputfolder: string) (codec: string) (inputfolder: string)=
    let files = Directory.EnumerateFiles inputfolder |> List.ofSeq
    files |> List.map (processFile codec outputfolder) |> Async.Parallel |> Async.RunSynchronously

let Run inputfolder outputfolder codec =
    // Settings
    let settings = System.Text.Json.JsonSerializer.Deserialize<ConversorSettings>(File.ReadAllText("settings.json"))
    let options = new FFOptions()
    options.BinaryFolder <- settings.ffmpegfolder
    GlobalFFOptions.Configure(options);

    // Processing directories
    let directories = IOService.GetDirectoriesWithContent inputfolder "*.flac" |> List.ofSeq
    directories |> List.map (ProcessFolder outputfolder codec) |> ignore
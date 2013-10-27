//namespace Energon.Charts

// Learn more about F# at http://fsharp.net
module Energon.Charts

open Energon.Measuring
open MSDN.FSharp.Charting
open System.Windows.Forms.DataVisualization.Charting
open System.Text
open System.Drawing

let showFloatLine (d:float[]) =
    d |> FSharpChart.FastLine |> FSharpChart.Create
    
let showReadingsLine (d:Reading[]) =
    d |> Array.map (fun r -> (r.Timestamp.ToLongTimeString(), r.Value) ) |> FSharpChart.FastLine |> FSharpChart.Create

let showFloatRealtime (e:IEvent<float>) =
    e |> FSharpChart.FastLine |> FSharpChart.Create

let showReadingRealtime (e:IEvent<Reading>) =
    e |> Observable.map (fun (r:Reading) -> (r.Timestamp, r.Value)) |> FSharpChart.FastLine |> FSharpChart.Create

let showSensorData (s:PushSensor) =
    FSharpChart.FastLine(Array.map (fun (r:Reading) -> (r.Timestamp, r.Value)) (s.Results), Name=s.Name) |> FSharpChart.WithLegend ( InsideArea = false, Alignment = StringAlignment.Center, Docking = Docking.Top) |> FSharpChart.Create

let showExperimentRunData (exp:ExperimentRun) =
    FSharpChart.Rows [ for s in exp.Sensors -> FSharpChart.FastLine(Array.map (fun (r:Reading) -> (r.Timestamp.Subtract(exp.StartTime).TotalSeconds, r.Value)) (exp.Results.[s]), Name=s.Name) :> ChartTypes.GenericChart] |> FSharpChart.WithMargin(0.0f, 8.0f, 2.0f, 0.0f) |> FSharpChart.WithLegend ( InsideArea = false, Alignment = StringAlignment.Center, Docking = Docking.Top) |> FSharpChart.Create 

let showSensorRealtime (s:PushSensor) =
    Observable.map (fun (r:Reading) -> (r.Timestamp, r.Value)) s.ObservableReadings |> FSharpChart.FastLine |> FSharpChart.Create

let showSensorListRealtime (sensors:seq<PushSensor>) =
    FSharpChart.Rows [ for s in sensors -> FSharpChart.FastLine(Observable.map (fun (r:Reading) -> (r.Timestamp, r.Value)) s.ObservableReadings, Name=s.Name) :> ChartTypes.GenericChart] |> FSharpChart.WithMargin(0.0f, 8.0f, 2.0f, 0.0f) |> FSharpChart.WithLegend ( InsideArea = false, Alignment = StringAlignment.Center, Docking = Docking.Top) |> FSharpChart.Create

let showExperimentCaseMeansAndStdDev (expCase:ExperimentCase) =
    let seq1 = Seq.map (fun s -> (s, expCase.MeansAndStdDev.[s])) expCase.Sensors
    let seq2 = Seq.map (fun ((s:GenericSensor), (m,d)) -> (s.Name, (m, d, d))) seq1
    let l = Seq.toArray seq2
    let errors = FSharpChart.ErrorBar l
    errors.ErrorBarStyle <- ChartStyles.ErrorBarStyle.Both
    errors.ErrorBarType <-ChartStyles.ErrorBarType.StandardDeviation
    let bars = FSharpChart.Column (Array.map (fun (s,(m,_,_))-> (s,m)) l)
    let combined = FSharpChart.Combine [bars ; errors]
    combined |> FSharpChart.Create

let showExperimentCaseMeansAndStdDevDetail (expCase:ExperimentCase) =
    let listOfListOfSensors = expCase.Sensors |> Seq.map (fun s -> (s, expCase.ResultsMeansAndStdDev.[s]))
    let fromlistToChart (s:GenericSensor) (l:System.Collections.Generic.List<float*float>) =
        let tripleSeq = l |> Seq.map (fun (m,d) -> (m,d,d))
        let triples = Seq.toArray tripleSeq
        let errorsChart = FSharpChart.ErrorBar triples
        errorsChart.ErrorBarType <-ChartStyles.ErrorBarType.StandardDeviation
        let columnsChart = FSharpChart.Column (Seq.toArray (Seq.map (fun (m,_) -> m) l))
        FSharpChart.Combine [columnsChart ; errorsChart] :> ChartTypes.GenericChart
    let rows = Seq.map (fun (s,l) -> fromlistToChart s l) listOfListOfSensors
    rows |> FSharpChart.Rows |> FSharpChart.Create

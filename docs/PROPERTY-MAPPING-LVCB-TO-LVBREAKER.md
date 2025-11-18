# Property Mapping: Old LVCB ? New LVBreaker
# For updating BreakerLabelGenerator.cs and other code

## Core Breaker Properties
- Manufacturer ? BreakerMfr
- Style ? BreakerStyle
- Type ? BreakerType
- FrameSize ? FrameA
- FrameRating ? FrameA  (same property)
- InterruptRating ? SCIntKA

## Trip Unit Properties  
- TripUnitManufacturer ? TripMfr
- TripUnitType ? TripType
- TripUnitStyle ? TripStyle
- TripUnitTripPlug ? PlugTapTrip
- TripUnitLtpuAmps ? TripA
- TripUnitLtpuMult ? LTPUMult
- TripUnitLtdBand ? LtdBand
- TripUnitLtdCurve ? LtCurve
- TripUnitStpuAmps ? STPUA
- TripUnitStpu ? STPUSetting
- TripUnitStdBand ? STPUBand
- TripUnitStdI2t ? STPUI2t
- TripUnitStpuI2t ? STPUI2t (same)
- TripUnitInstAmps ? InstA
- TripUnitInst ? InstSetting
- TripUnitTripAdjust ? TripAdjust
- TripUnitMaintSetting ? MaintSetting
- TripUnitMaintAmps ? MaintA
- TripUnitGfpuAmps ? GndA
- TripUnitGfd ? GndDelay
- TripUnitGfdI2t ? GndI2t
- TripUnitAdjustable ? Trip (inferred from "Trip" field)

## Special Mappings
- LVCB.Id ? LVBreaker.LVBreakers (first property)

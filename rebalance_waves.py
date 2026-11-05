import pathlib, re
root = pathlib.Path(r"C:\Unity\Magic Frontline\Assets\Scenes")
orc = "  orcEnemyPrefab: {fileID: 710020001, guid: 2f8a6c1d4b9e4a70c3d5e8f1a6b04792, type: 3}\n"
hard = "  hardEnemyPrefab: {fileID: 6882696952312109828, guid: ecf4a2841df41d346aa4bc9ebf3e071f, type: 3}\n"
place = "  placeSfx: {fileID: 8300000, guid: 749ab1432968eeb49bc97ef55725da70, type: 3}\n"
plans = {
 "Level_1.unity": {"coins": 10, "lanes": [
    [(18,8,0,0),(16,8,2,0),(15,6,4,0),(14,4,6,0),(12,4,8,0)],
 ]},
 "Level_2.unity": {"coins": 15, "lanes": [
    [(18,8,0,0),(16,6,4,0),(15,4,4,2),(14,4,4,2),(12,2,6,4)],
 ]},
 "Level_3.unity": {"coins": 20, "lanes": [
    [(18,6,0,0),(16,4,2,0),(15,2,2,2),(14,2,2,2),(12,2,2,2)],
    [(18,4,0,0),(16,4,0,0),(15,2,2,1),(14,2,2,1),(12,0,2,2)],
 ]},
 "Level_4.unity": {"coins": 25, "lanes": [
    [(18,4,0,0),(16,4,2,0),(15,2,2,1),(14,2,2,2),(13,0,2,2),(12,0,2,2)],
    [(18,4,0,0),(16,4,2,0),(15,2,2,1),(14,2,2,2),(13,0,2,2),(12,0,2,2)],
 ]},
 "Level_5.unity": {"coins": 30, "lanes": [
    [(18,4,0,0),(16,4,0,0),(15,2,2,0),(14,2,0,1),(13,0,2,1),(12,0,2,1)],
    [(18,4,0,0),(16,2,2,0),(15,2,2,1),(14,0,2,1),(13,0,2,1),(12,0,2,1)],
    [(18,2,0,0),(16,2,2,0),(15,2,0,1),(14,0,2,1),(13,0,2,0),(12,0,1,1)],
 ]},
}
pat = re.compile(r"  waves:\n(?:  - duration:.*\n    easyEnemies:.*\n    hardEnemies:.*\n(?:    orcEnemies:.*\n)?)+")

def waves_text(waves):
    lines = ["  waves:"]
    leak = 0
    for i, (d,e,h,o) in enumerate(waves):
        assert 0 < e+h+o <= 12, (e,h,o)
        if i < 2:
            assert o == 0
        leak += e*2 + h*3 + o*5
        lines += [f"  - duration: {d}", f"    easyEnemies: {e}", f"    hardEnemies: {h}", f"    orcEnemies: {o}"]
    return "\n".join(lines) + "\n", leak

for fname, plan in plans.items():
    p = root / fname
    text = p.read_text(encoding="utf-8")
    blocks = list(pat.finditer(text))
    assert len(blocks) == len(plan["lanes"]), (fname, len(blocks))
    out = text
    leaks = []
    for m, waves in zip(reversed(blocks), reversed(plan["lanes"])):
        block, leak = waves_text(waves)
        leaks.append(leak)
        out = out[:m.start()] + block + out[m.end():]
    leaks.reverse()
    out = out.replace(orc, "")
    if fname != "Level_1.unity":
        assert out.count(hard) == len(plan["lanes"])
        out = out.replace(hard, hard + orc)
    out = re.sub(r"  startCoins: \d+\n", "", out)
    assert out.count(place) == 1
    out = out.replace(place, place + f"  startCoins: {plan['coins']}\n", 1)
    p.write_text(out, encoding="utf-8", newline="\n")
    print(fname, plan["coins"], leaks, sum(leaks))
